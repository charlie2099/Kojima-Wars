using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIGroup
{
    public static readonly int MaxGroupsPerPlayer = 10;
    public static readonly int MaxAgentsPerGroup = 5;

    public BaseController BaseController = null;
    public List<GameObject> agents = new List<GameObject>();

    public AgentState state = AgentState.IDLE;
    public AttackMovement moveAttackState = AttackMovement.MOVE_TO_MEET_UP_POS;
    public DefenceMovement moveDefenceState = DefenceMovement.MOVE_TO_MEET_UP_POS;


    public UnitDefenceLocation groupUpLocationManager;
}

public class PlayerData
{
    public static readonly int BaseIdClearValue = -1;

    public PlayerData()
    {
        for(int i = 0; i < AIGroup.MaxGroupsPerPlayer; ++i)
        {
            aiGroups.Add(new AIGroup());
        }
    }

    public string Name { get; private set; } = "";
    public void SetName(string n) { Name = n; }

    public bool IsRedTeam { get; private set; } = false;
    public void SetTeamRed(bool isRed) { IsRedTeam = isRed; }

    // AI
    public List<AIGroup> aiGroups = new List<AIGroup>(AIGroup.MaxGroupsPerPlayer);

    // Bases
    public int baseIdInsideOf = BaseIdClearValue;
}

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
public class PlayerDataSO : ScriptableObject
{
    public Dictionary<ulong, PlayerData> List { get; private set; }

    private PlayerDataSO()
    {
        // WARNING : you cannot do this inline with the declaration
        List = new Dictionary<ulong, PlayerData>();
    }

    public PlayerData GetPlayerData(ulong id)
    {
        return List[id];
    }

    public bool GetPlayerTeam(ulong id)
    {
        return List[id].IsRedTeam;
    }   

    public string GetPlayerName(ulong id)
    {
        return List[id].Name;
    }

    public AIGroup GetAIGroup(ulong id, int groupIndex)
    {
        return List[id].aiGroups[groupIndex];
    }

    public List<AIGroup> GetAIGroups(ulong id)
    {
        return List[id].aiGroups;
    }

    public void SetAIGroup(ulong id, int groupIndex, AIGroup aiGroup)
    {
        List[id].aiGroups[groupIndex] = aiGroup;
    }

    // Helper function to add units to a player's ai group
    public void AddAgentToGroup(ulong id, int groupIndex, GameObject agent)
    {
        List[id].aiGroups[groupIndex].agents.Add(agent);
    }

    // Helper function to remove units from a player's ai group
    public void RemoveAgentFromGroup(ulong id, int groupIndex, GameObject agent)
    {
        List[id].aiGroups[groupIndex].agents.Remove(agent);
        // NOTE Remember to reset the base controller to null if the group is empty
    }

    // Helper function to set base controller in a player's ai group
    public void SetBaseControllerForGroup(ulong id, int groupIndex, BaseController controller)
    {
        List[id].aiGroups[groupIndex].BaseController = controller;
    }

    // Helper function to check if a group is empty
    public bool IsGroupEmpty(ulong id, int groupIndex)
    {
        return List[id].aiGroups[groupIndex].agents.Count == 0;
    }

    // Helper function to check if a group is full
    public bool IsGroupFull(ulong id, int groupIndex)
    {
        return List[id].aiGroups[groupIndex].agents.Count == AIGroup.MaxAgentsPerGroup;
    }

    public void SetBaseIdInsideOf(ulong id, int baseId)
    {
        List[id].baseIdInsideOf = baseId;
    }

    public int GetBaseIdInsideOf(ulong id)
    {
        return List[id].baseIdInsideOf;
    }

    public void ClearBaseIdInsideOf(ulong id)
    {
        List[id].baseIdInsideOf = PlayerData.BaseIdClearValue;
    }
}
