using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using System;
using System.Collections;
using UnityEngine.SceneManagement;
using Core;
using System.Threading.Tasks;

public enum WinCondition
{
    SCORE,
    BASES,
    TIMER
}

public class GameController : NetworkBehaviour
{
    [SerializeField] private GameStateDataSO m_gameStateData = default;
    [SerializeField] private GameTeamData m_gameTeamData = default;
    public PlayerDataSO m_playerData = default;
    [SerializeField] private GameTimerUI m_GameTimerUI = default;
    [SerializeField] private TeamScoreUI m_TeamScoreUI = default;
    [SerializeField] private UnitMapUI m_UnitMapUI = default;

    private AIGroup group;

    private bool m_gameFinished = false;

    public Counter GameTimer
    {
        get;
        private set;
    }

    public BaseController[] m_allBases = { };

    PlayerClassUI playerClassUI;

    [ServerRpc(RequireOwnership = false)]
    public void OnSelectAIGroupServerRpc(ulong localPlayerId, int groupIndex)
    {
        OnSelectAIGroupClientRpc(localPlayerId, groupIndex, m_playerData.GetAIGroup(localPlayerId, groupIndex).agents.Count > 0);
    }

    [ClientRpc]
    private void OnSelectAIGroupClientRpc(ulong selectingLocalPlayerId, int groupIndex, bool empty)
    {
        if (NetworkManager.Singleton.LocalClientId != selectingLocalPlayerId) return;

        if (empty)
        {
            m_UnitMapUI.OnSelectedGroupEmpty(groupIndex, selectingLocalPlayerId);
        }
        else
        {
            m_UnitMapUI.OnSelectedGroupNotEmpty();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateUnitInfoUIForGroupServerRpc(ulong localPlayerId, int groupIndex)
    {
        var numUnits = new Dictionary<EUnitTypes, int>() { { EUnitTypes.INFANTRY, 0 }, { EUnitTypes.TANK, 0 }, { EUnitTypes.HELICOPTER, 0 } };

        foreach (GameObject unit in m_playerData.GetAIGroup(NetworkManager.Singleton.LocalClientId, groupIndex).agents)
        {
            EUnitTypes type = unit.GetComponent<AI_AgentController>().UnitType;

            if (numUnits.ContainsKey(type))
            {
                numUnits[type] += 1;
            }
            else
            {
                numUnits.Add(type, 1);
            }
        }

        UpdateInfantryUnitInfoUIClientRpc(localPlayerId, numUnits[EUnitTypes.INFANTRY] != 0, numUnits[EUnitTypes.INFANTRY]);
        UpdateTankUnitInfoUIClientRpc(localPlayerId, numUnits[EUnitTypes.TANK] != 0, numUnits[EUnitTypes.TANK]);
        UpdateHelicopterUnitInfoUIClientRpc(localPlayerId, numUnits[EUnitTypes.HELICOPTER] != 0, numUnits[EUnitTypes.HELICOPTER]);
    }

    [ClientRpc]
    private void UpdateInfantryUnitInfoUIClientRpc(ulong requestingPlayerId, bool active, int quantity)
    {
        if (NetworkManager.Singleton.LocalClientId != requestingPlayerId) return;

        m_UnitMapUI.UpdateInfantryUI(active, quantity);
    }

    [ClientRpc]
    private void UpdateTankUnitInfoUIClientRpc(ulong requestingPlayerId, bool active, int quantity)
    {
        if (NetworkManager.Singleton.LocalClientId != requestingPlayerId) return;

        m_UnitMapUI.UpdateTankUI(active, quantity);
    }

    [ClientRpc]
    private void UpdateHelicopterUnitInfoUIClientRpc(ulong requestingPlayerId, bool active, int quantity)
    {
        if (NetworkManager.Singleton.LocalClientId != requestingPlayerId) return;

        m_UnitMapUI.UpdateHelicopterUI(active, quantity);
    }

    [ServerRpc(RequireOwnership = false)]
    public void MakeSelectorButtonForBaseInteractableServerRpc(ulong localPlayerId, int baseId, int groupIndex)
    {
        MakeSelectorButtonForBaseInteractableClientRpc(localPlayerId, baseId, m_playerData.GetAIGroup(localPlayerId, groupIndex).agents.Count > 0);
    }

    [ClientRpc]
    private void MakeSelectorButtonForBaseInteractableClientRpc(ulong requestingPlayerId, int baseId, bool interactable)
    {
        if (NetworkManager.Singleton.LocalClientId != requestingPlayerId) return;

        m_UnitMapUI.MakeSelectorButtonForBaseInteractable(baseId, interactable);
    }

    [ServerRpc(RequireOwnership = false)]
    public void MakeUnitGroupButtonInteractableServerRpc(ulong localPlayerId, int groupIndex)
    {
        if(m_playerData.GetAIGroup(localPlayerId, groupIndex).agents.Count > 0)
        {
            MakeUnitGroupButtonInteractableClientRpc(localPlayerId, groupIndex, true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void CheckAIGroupButtonValidServerRpc(ulong localPlayerId, int groupIndex)
    {
        AIGroup aiGroup = m_playerData.GetAIGroup(localPlayerId, groupIndex);

        bool groupContainsAgents = aiGroup.agents.Count != 0;
        MakeUnitGroupButtonInteractableClientRpc(localPlayerId, groupIndex, groupContainsAgents);

        if(!groupContainsAgents)
        {
            m_UnitMapUI.DeselectButton(groupIndex);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SelectAIGroupServerRpc(ulong localPlayerId, int groupIndex)
    {
        AIGroup group = m_playerData.GetAIGroup(localPlayerId, groupIndex);

        if (group.BaseController != null)
        {
            SelectBaseSelectionIconsClientRpc(localPlayerId, group.BaseController.GetBaseId());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SelectBaseAIMapServerRpc(ulong localPlayerId, int baseId, int groupIndex)
    {
        // Get the selected base controller
        BaseController selectedBaseController = null;
        foreach(BaseController baseController in m_allBases)
        { 
            if(baseId == baseController.GetBaseId())
            {
                selectedBaseController = baseController;
                break;
            }
        }

        Debug.Log("Selected map: " + selectedBaseController.name);



        // Get group data from player data
        AIGroup group = m_playerData.GetAIGroup(localPlayerId, groupIndex);

        // Set base controller ai group are at
        m_playerData.SetBaseControllerForGroup(localPlayerId, groupIndex, selectedBaseController);


        bool meetUp = false;
        
        foreach (BaseController baseController in m_allBases)
        {
            foreach (GameObject agent in group.agents)
            {
                if (Vector3.Distance(baseController.transform.position, agent.transform.position) < 100)
                {
                    group.groupUpLocationManager = baseController.m_meetUpLocationManager;
                    meetUp = true;
                    break;
                }
            }
        }

        // Move agents in group to the base controller
        foreach (GameObject agent in group.agents)
        {
            agent.GetComponent<AI_AgentController>().group = group;
            if (m_playerData.GetPlayerTeam(localPlayerId) && selectedBaseController.TeamOwner == "red" ||
              !m_playerData.GetPlayerTeam(localPlayerId) && selectedBaseController.TeamOwner == "blue")
            {
                group.state = AgentState.DEFENDING;
                group.moveDefenceState = DefenceMovement.AT_DEFENCE_POS;
                Debug.Log("Move To Defence Point");
            }
            else
            {
                group.state = AgentState.ATTACKING;
                group.moveAttackState = AttackMovement.MOVE_TO_MEET_UP_POS;
                agent.GetComponent<AI_AgentController>().SetLocalPlayerId(localPlayerId);
            }
        }

        FindObjectOfType<AI_Mover>().MoveToInitialBasePos(group, meetUp);
    }

    public void SetAgentGroup(ulong localPlayerId, int groupIndex, UnitDefenceLocation meetUpLocation, bool meetUp)
    {
          group = m_playerData.GetAIGroup(localPlayerId, groupIndex);
          


          //SetAgentGroupClientRpc(localClientId);
    }

    [ClientRpc]
    private void SetAgentGroupClientRpc(ulong localClientId)
    {
        foreach (var object_ in FindObjectsOfType<NetworkObject>())
        {
            if (object_.NetworkObjectId == localClientId)
            {
                object_.GetComponent<AI_AgentController>().group = group;
            }
        }
    }



    [ServerRpc(RequireOwnership = false)]
    public void GetAIGroupServerRpc(ulong localClientId, int i)
    {
        if (NetworkManager.Singleton.LocalClientId != localClientId) return;
        m_UnitMapUI.aiGroup = m_playerData.GetAIGroup(localClientId, i);
    }

    [ClientRpc]
    private void SelectBaseSelectionIconsClientRpc(ulong localPlayerId, int baseId)
    {
        if (NetworkManager.Singleton.LocalClientId != localPlayerId) return;
        m_UnitMapUI.SetBaseSelectionIcons(localPlayerId, baseId);
    }

    [ClientRpc]
    public void MakeUnitGroupButtonInteractableClientRpc(ulong requestingLocalClientId, int buttonIndex, bool interactable)
    {
        if (NetworkManager.Singleton.LocalClientId != requestingLocalClientId) return;

        m_UnitMapUI.MakeUnitGroupButtonInteractable(buttonIndex, interactable);
    }

    [ServerRpc]
    public void AddScoreToTeamServerRpc(string teamName, int scoreToAdd)
    {
        GameTeamData.TeamData teamData = m_gameTeamData.GetTeamData(teamName);
        teamData.Score += scoreToAdd;
        m_gameTeamData.SetTeamData(teamName, teamData);

        m_TeamScoreUI.OnTeamScoreUpdated(teamName, teamData.Score);
        UpdateScoreClientRpc(teamName, teamData.Score);

        if (teamData.Score >= m_gameStateData.scoreThreshold)
        {
            HandleEndGame(WinCondition.SCORE, teamName);
        }
    }

    [ClientRpc]
    private void UpdateScoreClientRpc(string teamName, int score)
    {
        if (IsServer) return;

        m_TeamScoreUI.OnTeamScoreUpdated(teamName, score);
    }

    [ServerRpc]
    public void PauseGameTimerServerRpc()
    {
        GameTimer.Pause();
    }

    [ServerRpc]
    public void ResumeGameTimerServerRpc()
    {
        GameTimer.Resume();
    }

    private void Awake()
    {
        CursorManager.DisableCursor("main-menu");
        m_allBases = FindObjectsOfType<BaseController>();
        foreach (BaseController baseController in m_allBases)
        {
            baseController.OnOwnerChanged += OnBaseChangedOwner;
        }
    }

    private void Start()
    {
        if(IsServer)
        {
            GameTimer = new Counter(this, m_gameStateData.maxGameLength, Counter.TimerType.MINUTES);
            GameTimer.OnComplete += OnGameTimerComplete;
            GameTimer.OnChanged += OnGameTimerChangedServerRpc;
            GameTimer.StartTimer();
        }
        playerClassUI = GameObject.Find("PlayerClassUI").GetComponent<PlayerClassUI>();
    }

    private void OnDisable()
    {
        if(IsServer)
        {
            GameTimer.OnComplete -= OnGameTimerComplete;
        }

        foreach (BaseController baseController in m_allBases)
        {
            baseController.OnOwnerChanged -= OnBaseChangedOwner;
        }
    }

    private void OnBaseChangedOwner(string newOwner)
    {
        string previousName = null;
        foreach (BaseController baseController in m_allBases)
        {
            if (previousName != null && baseController.TeamOwner != previousName)
            {
                return;
            }

            previousName = baseController.TeamOwner;
        }

        HandleEndGame(WinCondition.BASES, previousName);
    }

    [ServerRpc]
    private void OnGameTimerChangedServerRpc()
    {
        if (GameTimer.IsCounting)
        {
            float timeRemaining = GameTimer.TimeRemaining;
            float secondsRemaining = timeRemaining % 60;
            float minutesRemaining = (timeRemaining - secondsRemaining) / 60;
            UpdateGameTimeClientRpc(minutesRemaining, secondsRemaining);
        }
    }

    [ClientRpc]
    private void UpdateGameTimeClientRpc(float minutesRemaining, float secondsRemaining)
    {
        string text = $"{minutesRemaining.ToString("00")}:{secondsRemaining.ToString("00")}";
        m_GameTimerUI.UpdateTime(text);
    }

    private void OnGameTimerComplete()
    {
        string winner = null;

        int bestScore = 0;
        // Key is team name and value is team data
        Dictionary<string, GameTeamData.TeamData> teamDataDictionary = m_gameTeamData.GetTeamDataDictionary();
        foreach (KeyValuePair<string, GameTeamData.TeamData> keyValuePair in teamDataDictionary)
        {
            if(keyValuePair.Value.Score > bestScore)
            {
                bestScore = keyValuePair.Value.Score;
                winner = keyValuePair.Key;
            }
        }

        HandleEndGame(WinCondition.TIMER, winner);
    }

    private void HandleEndGame(WinCondition condition,  string winningTeam)
    {
        GameTimer.Pause();

        // pause all game input
        GameObject.Find("PauseManager").GetComponent<PauseManager>().Pause(false);

        /*
        if (GameObject.Find("PauseUI").GetComponent<PauseUIManager>().UIActive)
        {
            GameObject.Find("PauseUI").GetComponent<PauseUIManager>().HidePauseUI();
        }
        if (playerClassUI.isActiveAndEnabled)
        {
            playerClassUI.ShowUI(false);
        }
        */
        InputManager.SetInputType(ControlType.NONE);


        EndGameServerRpc(condition, winningTeam);
    }

    [ServerRpc(RequireOwnership =false)]
    private void EndGameServerRpc(WinCondition condition, FixedString32Bytes winningTeam)
    {
        if(!m_gameFinished)
        {
            m_gameFinished = true;
            // TODO Authorize game has ended on server (Score has been reached)
            //Debug.Log("Server handling end game for winning team: " + winningTeam);
            ShowGameFinishedUIClientRpc(condition, winningTeam);
        }
    }

    [ClientRpc]
    private void ShowGameFinishedUIClientRpc(WinCondition condition, FixedString32Bytes winningTeam)
    {
        var GameFinishedUI = GameObject.Find("GameOverUI");
        GameFinishedUI.GetComponent<GameFinishedUIController>().OnGameEnd(condition, winningTeam.ConvertToString());
        InputManager.SetInputType(ControlType.NONE);
    }

    public void ExitGame()
    {
        //Debug.Log("Exiting game");

        ExitGameServerRpc(NetworkManager.Singleton.LocalClientId);
        
    }

    [ServerRpc(RequireOwnership = false)]
    private void ExitGameServerRpc(ulong clientID)
    {
        if (clientID == NetworkManager.Singleton.ServerClientId)
        {
            ExitGameAllClientRpc();
            DisconnectAfterAllClientsAsync();
        }
        else
        {
            ExitGameSingleClientRpc(clientID);
        }
    }

    private async void DisconnectAfterAllClientsAsync()
    {
        while(NetworkManager.Singleton.ConnectedClients.Count > 1)
        {
            await Task.Delay(500);
        }
        
        Networking.NetworkLibrary.EndSession();
        ReloadGame();
    }

    [ClientRpc]
    private void ExitGameAllClientRpc()
    {
        if (IsServer) return;
        CursorManager.ForceEnableCursor();
        Networking.NetworkLibrary.LeaveSession();
        ReloadGame();
        m_gameTeamData.OnDestroy();
    }

    [ClientRpc]
    private void ExitGameSingleClientRpc(ulong clientID)
    {
        if(clientID == NetworkManager.Singleton.LocalClientId)
        {
            Networking.NetworkLibrary.LeaveSession();
            ReloadGame();
        }
    }

    private void ReloadGame()
    {
        Destroy(NetworkManager.Singleton.gameObject);
        Destroy(AudioManager.Instance.gameObject);
        Destroy(SceneLoader.Instance.gameObject);
        Destroy(CursorManager.Instance.gameObject);
        TransformManager.ClearAll();
        SceneManager.LoadScene("SplashScene");
    }
}