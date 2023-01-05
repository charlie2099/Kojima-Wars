using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BaseCaptureZone : NetworkBehaviour
{
    public BaseController BaseController => m_baseController;
    [SerializeField] private BaseController m_baseController;

    [SerializeField] private GameObject m_laserObject;
    [SerializeField] private DefenceLocationManager m_defenceManager;

    private float m_captureProgress = 0f;
    private int m_captureProgressOwnerId = -1;

    [SerializeField] private GameTeamData m_teamData;
    [SerializeField] private PlayerDataSO m_playerData;
    [SerializeField] private GameStateDataSO m_gameStateData;

    private Dictionary<string, List<ulong>> m_playersInZone = new Dictionary<string, List<ulong>>();
    private Dictionary<string, List<Entity>> m_unitsInZone = new Dictionary<string, List<Entity>>();

    // list of players in this zone - 0 is red team players, 1 is blue team players
    private Dictionary<int, List<ulong>> teamPlayerIds = new Dictionary<int, List<ulong>>();

    // num units in zone per team - key is team id and value is unit count
    private Dictionary<int, int> teamUnitCounts = new Dictionary<int, int>();

    private Color m_neutralBaseColor = Color.gray;
    private string m_ownerProperty = "_Owner";

    private void Start()
    {
        if (IsServer)
        {
            StartServer();
            return;
        }
        else
        {
            StartClient();
            return;
        }
    }

    private void StartServer()
    {
        // Initialize team dictionaries
        for (int i = 0; i < 2; ++i)
        {
            teamPlayerIds[i] = new List<ulong>();
            teamUnitCounts[i] = 0;
        }

        SetBaseLaserColorClientRpc(m_neutralBaseColor);
    }

    private void StartClient()
    {
        // Disable zone collider on clients
        if (!TryGetComponent(out Collider zoneCollider)) return;
        zoneCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!TryGetEntityFromCollider(other, out var entity)) return;

        switch (entity.EntityType)
        {
            case Entity.EEntityType.PLAYER: OnPlayerEnter(entity); break;
            case Entity.EEntityType.UNIT: OnUnitEnter(entity); break;
            default: break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!TryGetEntityFromCollider(other, out var entity)) return;

        switch (entity.EntityType)
        {
            case Entity.EEntityType.PLAYER: OnPlayerExit(entity); break;
            case Entity.EEntityType.UNIT: OnUnitExit(entity); break;
            default: break;
        }
    }

    private bool TryGetEntityFromCollider(Collider collider, out Entity entity)
    {
        entity = collider.gameObject.GetComponent<Entity>();
        return entity != null;
    }

    private void OnPlayerEnter(Entity entity)
    {
        if (!IsServer) return;

        int teamId = GetTeamID(entity);

        if (teamPlayerIds[teamId].Contains(entity.OwnerClientId)) return;

        teamPlayerIds[teamId].Add(entity.OwnerClientId);

        EnableClientCaptureUIClientRpc(entity.OwnerClientId, true);
    }

    private void OnUnitEnter(Entity entity)
    {
        if (!IsServer) return;

        int teamId = GetTeamID(entity);

        ++teamUnitCounts[teamId];
    }

    private void OnPlayerExit(Entity entity)
    {
        if (!IsServer) return;

        int teamId = GetTeamID(entity);

        if (!teamPlayerIds[teamId].Contains(entity.OwnerClientId)) return;

        teamPlayerIds[teamId].Remove(entity.OwnerClientId);

        EnableClientCaptureUIClientRpc(entity.OwnerClientId, false);
    }

    private void OnUnitExit(Entity entity)
    {
        if (!IsServer) return;

        int teamId = GetTeamID(entity);

        --teamUnitCounts[teamId];
    }

    private int GetTeamID(Entity entity)
    {
        return entity.TeamName == m_teamData.GetTeamDataAtIndex(0).TeamName ? 0 : 1;
    }

    private int GetOwningTeamID()
    {
        if (BaseController.TeamOwner == "") return -1;

        return BaseController.TeamOwner == m_teamData.GetTeamDataAtIndex(0).TeamName ? 0 : 1;
    }

    private string GetTeamString(int teamId)
    {
        if (teamId == -1) return "";

        return m_teamData.GetTeamDataAtIndex(teamId).TeamName;
    }

    private int GetOpposingTeamID(int owningTeamId)
    {
        return owningTeamId == 0 ? 1 : 0;
    }

    private int GetNumberOfPlayersFromTeamInZone(int teamId)
    {
        if (teamId == -1) return 0;

        return teamPlayerIds[teamId].Count;
    }

    private const float UIUpdateFrequency = 100; //ms
    private float timer = 0;

    private void Update()
    {
        if (!IsServer) return;

        RemoveDeadPlayerObjects();

        switch (BaseController.State)
        {
            case EBaseState.IDLE: UpdateIdle(); break;
            case EBaseState.CONTESTED: UpdateContested(); break;
            case EBaseState.UNCONTESTED: UpdateUncontested(); break;
        }

        timer -= Time.deltaTime;
        if (timer < 0)
        {
            for (int i = 0; i < 2; i++)
            {
                foreach (var player in teamPlayerIds[i])
                {
                    UpdateClientCaptureUIClientRpc(player, m_captureProgressOwnerId, m_captureProgress);
                }
            }
        }

    }

    private void RemoveDeadPlayerObjects()
    {
        foreach (KeyValuePair<int, List<ulong>> pair in teamPlayerIds)
        {
            foreach (var cc in CombatComponent.List)
            {
                if (!pair.Value.Contains(cc.OwnerClientId)) continue;
                if (cc.IsAlive()) continue;

                pair.Value.Remove(cc.OwnerClientId);
            }
        }
    }

    private void UpdateIdle()
    {
        // Check if any players or units are inside the zone
        if (!(DoesZoneContainAnyPlayers() || DoesZoneContainAnyUnits())) return;

        if (OwningTeamArePresent() && !OpposingTeamArePresent())
        {
            // regain progress when in your own base
            var id = GetOwningTeamID();
            if (m_captureProgress > 0.999f || id == -1) return;
            UpdateCaptureProgress(id, 1);
            return;
        }

        // Check if need to switch state
        // If both teams are present
        if (AreBothTeamsInsideZone())
        {
            // Enter contested
            BaseController.ChangeState(EBaseState.CONTESTED);
            return;
        }

        // Only one team is present
        // Enter uncontested
        BaseController.ChangeState(EBaseState.UNCONTESTED);
        return;

    }

    private void UpdateCaptureProgress(int id, int scalar)
    {
        m_captureProgress += ComputeCaptureRate(id) * scalar * Time.deltaTime;
        m_captureProgress = Mathf.Clamp(m_captureProgress, 0f, 1f);
    }

    private void UpdateContested()
    {
        // if current owning team players present or no players present
        if ((OwningTeamArePresent() && !OpposingTeamArePresent()) || !(DoesZoneContainAnyPlayers() || DoesZoneContainAnyUnits()))
        {
            // switch to idle
            BaseController.ChangeState(EBaseState.IDLE);
            return;
        }
        // Go to uncontested if there is only one team and they are not the owner
        else if (IsOneTeamInsideZone())
        {
            // switch to Uncontested
            BaseController.ChangeState(EBaseState.UNCONTESTED);
            return;
        }
    }

    private void UpdateUncontested()
    {
        var id = GetOnlyTeamInArea();

        if (m_captureProgress < 0.001f)
        {
            // remove ownership
            SetBaseLaserColorClientRpc(m_neutralBaseColor);
            ChangeBaseTeamOwner(-1);
            // set progress owner 
            m_captureProgressOwnerId = id;
        }

        // If capture progress is 1 
        if (m_captureProgress > 0.999f && id == m_captureProgressOwnerId)
        {
            // Change base owner to contesting team
            SetBaseLaserColorClientRpc(m_teamData.GetTeamDataAtIndex(m_captureProgressOwnerId).Colour);
            ChangeBaseTeamOwner(m_captureProgressOwnerId);
        }

        // Check whether to switch state
        // If team 0 and 1 have players inside the zone
        if (AreBothTeamsInsideZone())
        {
            // Swtich to contested state
            BaseController.ChangeState(EBaseState.CONTESTED);
            return;
        }

        // If there are no players in the zone
        if (!(DoesZoneContainAnyPlayers() || DoesZoneContainAnyUnits()) || id == GetOwningTeamID())
        {
            // Switch to idle
            BaseController.ChangeState(EBaseState.IDLE);
            return;
        }

        // Update capture progress (if owner is -1 go up, otherwise go down)
        if (m_captureProgressOwnerId == id)
        {
            UpdateCaptureProgress(id, 1);
        }
        else
        {
            UpdateCaptureProgress(id, -1);
        }
    }

    private float ComputeCaptureRate(int teamId)
    {
        return (m_gameStateData.perPlayerCaptureRate * teamPlayerIds[teamId].Count) +
                (m_gameStateData.perUnitCaptureRate * teamUnitCounts[teamId]);
    }

    private bool DoesZoneContainAnyPlayers()
    {
        return teamPlayerIds[0].Count != 0 || teamPlayerIds[1].Count != 0;
    }

    private bool DoesZoneContainAnyUnits()
    {
        return teamUnitCounts[0] != 0 || teamUnitCounts[1] != 0;
    }

    private bool DoesZoneContainUnitsOnTeam(int teamId)
    {
        if (teamId == -1) return false;
        return teamUnitCounts[teamId] != 0;
    }

    private bool AreBothTeamsInsideZone()
    {
        return ((teamPlayerIds[0].Count + teamUnitCounts[0]) != 0) && ((teamPlayerIds[1].Count + teamUnitCounts[1]) != 0);
    }

    private bool IsOneTeamInsideZone()
    {
        return ((teamPlayerIds[0].Count + teamUnitCounts[0]) != 0 && (teamPlayerIds[1].Count + teamUnitCounts[1]) == 0) ||
            ((teamPlayerIds[1].Count + teamUnitCounts[1]) != 0 && (teamPlayerIds[0].Count + teamUnitCounts[0]) == 0);
    }

    private bool OwningTeamArePresent()
    {
        var teamID = GetOwningTeamID();
        if (teamID == -1) return false;
        return (teamPlayerIds[teamID].Count + teamUnitCounts[0]) > 0;
    }

    private bool OpposingTeamArePresent()
    {
        var ownerTeamId = GetOwningTeamID();
        if (ownerTeamId == -1) return true;
        var opposingTeamID = GetOpposingTeamID(ownerTeamId);
        return (teamPlayerIds[opposingTeamID].Count + teamUnitCounts[opposingTeamID]) > 0;
    }

    private int GetOnlyTeamInArea()
    {
        if (!(DoesZoneContainAnyPlayers() || DoesZoneContainAnyUnits())) return -1;
        return (teamPlayerIds[0].Count + teamUnitCounts[0]) > (teamPlayerIds[1].Count + teamUnitCounts[1]) ? 0 : 1;
    }

    private void ChangeBaseTeamOwner(int newTeamOwnerId)
    {
        BaseController.ChangeTeamOwner(GetTeamString(newTeamOwnerId));
        SetUnitDefenceInfo();
    }

    private void SetUnitDefenceInfo()
    {
        for (int x = 0; x < 5; x++)
        {
            for (int i = 0; i < 5; i++)
            {
                DefenceLocationInfo locationInfo = m_defenceManager.unitDefenceLocations[i].defenceLocationInfo[x];
                Debug.Log(locationInfo);
                if (locationInfo.enemyOccupied)
                {
                    if (locationInfo.enemyUnit.GetComponent<Entity>().TeamName == m_baseController.TeamOwner)
                    {
                        locationInfo.enemyUnit.GetComponent<AI_AgentController>().SetNewTeamDefenceLocationInfo(m_baseController.GetBaseId(), i, x);
                    }
                }
            }
        }
    }

    // Network functionality
    [ClientRpc]
    private void EnableClientCaptureUIClientRpc(ulong localPlayerId, bool enable)
    {
        BaseCaptureUI1.Singleton.OnEnableUI(localPlayerId, enable);
    }

    [ClientRpc]
    private void UpdateClientCaptureUIClientRpc(ulong localPlayerId, int teamId, float captureProgress)
    {
        BaseCaptureUI1.Singleton.UpdateUI(localPlayerId, teamId, captureProgress);
    }

    [ClientRpc]
    private void SetBaseLaserColorClientRpc(Color color)
    {
        m_laserObject.GetComponent<Renderer>().material.SetColor(Shader.PropertyToID(m_ownerProperty), color);
    }
}