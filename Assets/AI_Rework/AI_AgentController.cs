using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using Unity.Collections;
using UnityEngine.Serialization;
using System.Collections;

public enum AgentState
{
    IDLE,
    ATTACKING,
    DEFENDING
}
public enum AttackMovement
{
    MOVE_TO_MEET_UP_POS,
    MOVE_TO_NEW_BASE_POS,
    MOVE_TO_ATTACK_POS,
    AT_ATTACK_POS
}
public enum DefenceMovement
{
    MOVE_TO_MEET_UP_POS,
    MOVE_TO_NEW_BASE_POS,
    AT_DEFENCE_POS
}

public class AI_AgentController : NetworkBehaviour, IDamageable
{
    public EUnitTypes UnitType => unitType;

    [SerializeField] private GameTeamData teamData;

    public DefenceLocationManager defenceLocationManager;
    public AgentState agentState = AgentState.DEFENDING;
    public AttackMovement moveAttackState = AttackMovement.MOVE_TO_MEET_UP_POS;
    public bool move;
    public bool meetUpMove;
    public bool moveToNextPos;
    public bool capturedBase;

    private NavMeshAgent navMeshAgent;
    private EUnitTypes unitType;
    private Entity entityComponent;
    private int damageStrength;
    private int groupIndex;
    private int health;
    private NetworkObjectReference modelObject;
    private NetworkObjectReference agentObject;
    public DefenceLocationManager lastDefenceLocationManager;
    private DefenceLocationManager resetDefenceLocationManager;
    public UnitDefenceLocation groupUpLocationManager;

    public AIGroup group;

    public Vector3 meetUpPos;
    public Vector3 attackPos;
    public Vector3 patrolPos;
    public Vector3 previousPatrolPos;
    public int patrolIndex;

    public int newBaseId;

    public ulong newLocalPlayerId;

    public override void OnNetworkSpawn()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        entityComponent = GetComponent<Entity>();
        navMeshAgent.speed = 40;
        navMeshAgent.acceleration = 300;
    }

    [ServerRpc]
    public void TakeDamageServerRpc(int damage)
    {
        health -= damage;
        if(!IsAlive())
        {
            OnDeath();
        }
    }

    [ServerRpc]
    public void HealDamageServerRpc(int heal)
    {
        // Does nothing as AI units cannot be healed
    }

    public bool IsAlive()
    {
        return health > 0;
    }
    public bool IsEnabled() => gameObject.activeInHierarchy;
    public bool IsRecalling() => false;
    [ServerRpc(RequireOwnership = false)]
    public void SetNavMeshAgentDestinationServerRpc(Vector3 position)
    {
        SetNavMeshDestination(position);
    }

    [ServerRpc(RequireOwnership = false)]
    public void StopNavMeshAgentMovementServerRpc()
    {
        StopNavMeshAgentMovement();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ResumeNavMeshAgentMovementServerRpc()
    {
        ResumeNavMeshAgentMovement();
    }

    public void OnDeath()
    {
        if (!IsOwner) return;
        AIUnitManager unitManager = GameObject.FindGameObjectWithTag("AI_Manager").GetComponent<AIUnitManager>();
        unitManager.OnAgentDeathServerRpc(NetworkManager.Singleton.LocalClientId, groupIndex, agentObject);
        DestroyAgentServerRpc();
    }

    public bool CheckPosition(Vector3 position)
    {
        if(Vector3.Distance(transform.position, position) < 4)
        {
            return true;
        }
        return false;
    }

    public void SetLastDefenceLocationManager()
    {
        lastDefenceLocationManager = defenceLocationManager;
    }

    public void SetDefenceLocationManager(int baseId)
    {
        foreach(BaseController baseController in FindObjectOfType<GameController>().m_allBases)
        { 
            if(baseId == baseController.GetBaseId())
            {
                defenceLocationManager = baseController.m_defenceLocationManager;
                break;
            }
        }
    }





    [ServerRpc]
    public void SetDefenceLocationInfoServerRpc(bool attacking)
    {
        move = true;
        for (int x = 0; x < 5; x++)
        {
            for (int i = 0; i < 5; i++)
            {
                DefenceLocationInfo locationInfo = defenceLocationManager.unitDefenceLocations[i].defenceLocationInfo[x];
                if (!attacking)
                {
                    if (!locationInfo.occupied && move)
                    {
                        SetNavMeshDestination(locationInfo.location);
                        RemoveDefenceLocationInfo(groupIndex, newLocalPlayerId);

                        locationInfo.occupied = true;
                        locationInfo.groupId = groupIndex;
                        locationInfo.unit = gameObject;
                        locationInfo.localPlayerId = newLocalPlayerId;
                        move = false;
                        break;
                    }
                }
                else
                {
                    if (!locationInfo.enemyOccupied && move)
                    {
                        attackPos = locationInfo.location;
                        locationInfo.enemyOccupied = true;
                        locationInfo.enemyGroupId = groupIndex;
                        locationInfo.enemyUnit = gameObject;
                        locationInfo.enemyLocalPlayerId = newLocalPlayerId;
                        move = false;
                        break;
                    }
                }
            }
        }
    }
    
    [ClientRpc]
    public void MoveToDefendPositionClientRpc(int groupIndex, int baseId, ulong localPlayerId, bool meetUp)
    {
        //FindObjectOfType<GameController>().SetAgentGroupServerRpc(localPlayerId, NetworkObjectId, groupIndex);
        agentState = AgentState.DEFENDING;
        move = true;
        SetDefenceLocationManager(baseId);

        for (int x = 0; x < 5; x++)
        {
            for (int i = 0; i < 5; i++)
            {
                DefenceLocationInfo locationInfo = defenceLocationManager.unitDefenceLocations[i].defenceLocationInfo[x];
                if (!locationInfo.occupied && move)
                {
                    SetNavMeshDestination(locationInfo.location);
                    RemoveDefenceLocationInfo(groupIndex, localPlayerId);

                    locationInfo.occupied = true;
                    locationInfo.groupId = groupIndex;
                    locationInfo.unit = gameObject;
                    locationInfo.localPlayerId = localPlayerId;
                    move = false;
                    break;
                }
            }
        }
    }

    public void SetNewTeamDefenceLocationInfo(int baseId, int i, int x)
    {
        foreach (BaseController baseController in FindObjectOfType<GameController>().m_allBases)
        {
            if (baseId == baseController.GetBaseId())
            {
                resetDefenceLocationManager = baseController.m_defenceLocationManager;
            }
        }
        
        DefenceLocationInfo locationInfo = resetDefenceLocationManager.unitDefenceLocations[i].defenceLocationInfo[x];
        locationInfo.occupied = true;
        locationInfo.unit = locationInfo.enemyUnit;
        locationInfo.groupId = locationInfo.enemyGroupId;
        locationInfo.localPlayerId = locationInfo.enemyLocalPlayerId;

        if(locationInfo.unit == gameObject)
        {
            SetNavMeshDestination(locationInfo.location);
        }

        locationInfo.enemyOccupied = false;
        locationInfo.enemyUnit = null;
        locationInfo.enemyGroupId = -1;
        locationInfo.enemyLocalPlayerId = 100;
    }
    public void RemoveDefenceLocationInfo(int groupIndex, ulong localPlayerId)
    {
        if(lastDefenceLocationManager != null)
        {
            for (int x = 0; x < 5; x++)
            {
                for (int i = 0; i < 5; i++)
                {
                    DefenceLocationInfo locationInfo = lastDefenceLocationManager.unitDefenceLocations[i].defenceLocationInfo[x];

                    if (locationInfo.groupId == groupIndex && locationInfo.localPlayerId == localPlayerId)
                    {
                        locationInfo.occupied = false;
                        locationInfo.groupId = -1;
                        locationInfo.unit = null;
                        locationInfo.localPlayerId = 100;
                    }
                    else if (locationInfo.enemyGroupId == groupIndex && locationInfo.enemyLocalPlayerId == localPlayerId)
                    {
                        locationInfo.enemyOccupied = false;
                        locationInfo.enemyGroupId = -1;
                        locationInfo.enemyUnit = null;
                        locationInfo.enemyLocalPlayerId = 100;
                    }
                }
            }
        }
    }


    public void SetUnitType(EUnitTypes type)
    {
        if (!IsServer) return;

        unitType = type;
    }

    public EUnitTypes GetUnitType()
    {
        return unitType;
    }

    public void SetEntityTeamName(FixedString32Bytes teamName)
    {
        if (!IsServer) return;

        entityComponent.ChangeTeamClientRpc(teamName);

        // Set the AI agents to the correct layer mask for their team
        // NOTE Red team must come first in the team data scriptable object
        // 25 - AI_RED
        // 26 - AI_BLUE
        int redLayer = 25;
        int blueLayer = 26;

        // If the team name being set to is the same as the first team's in the team data SO. Assuming that this will be red team
        if(teamName.ToString() == teamData.GetTeamDataAtIndex(0).TeamName)
        {
            gameObject.layer = redLayer;
        }
        else
        {
            gameObject.layer = blueLayer;
        }
    }

    public void SetDamageStrength(int strength)
    {
        if (!IsServer) return;

        damageStrength = strength;
    }

    public int GetDamageStrength()
    {
        return damageStrength;
    }

    public void SetHealth(int value)
    {
        if (!IsServer) return;

        health = value;
    }

    public int GetHealth()
    {
        return health;
    }

    public void SetGroupIndex(int index)
    {
        if (!IsServer) return;

        groupIndex = index;
    }

    public void SetLocalPlayerId(ulong localPlayerId)
    {
        newLocalPlayerId = localPlayerId;
    }

    public int GetGroupIndex()
    {
        return groupIndex;
    }

    public void SetModelObjectReference(NetworkObjectReference reference)
    {
        if (!IsServer) return;

        modelObject = reference;
    }

    public void SetAgentObjectReference(NetworkObjectReference reference)
    {
        if (!IsServer) return;

        agentObject = reference;
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyAgentServerRpc()
    {
        NetworkObject model = null;
        modelObject.TryGet(out model);
        Destroy(model.gameObject);

        Destroy(gameObject);
    }

    // NOTE Only call this function from the server
    private void SetNavMeshDestination(Vector3 position)
    {
        if (!IsServer) return;
        //Debug.Log("Setting nav mesh destination: " + position + (IsServer ? " [Server]" : " [Client]"));
        navMeshAgent.SetDestination(position);
        navMeshAgent.speed = 20;
        navMeshAgent.acceleration = 400;
    }

    // NOTE Only call this function from the server
    private void StopNavMeshAgentMovement()
    {
        if (!IsServer) return;
        navMeshAgent.isStopped = true;
    }

    // NOTE Only call this function from the server
    private void ResumeNavMeshAgentMovement()
    {
        if (!IsServer) return;
        navMeshAgent.isStopped = false;
    }
}
