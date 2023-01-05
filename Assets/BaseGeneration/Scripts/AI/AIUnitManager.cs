using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

[RequireComponent(typeof(NetworkObject))]
public class AIUnitManager : NetworkBehaviour
{
    [SerializeField] GameObject aiAgentPrefab;
    [SerializeField] private AIUnitTypesData unitTypesData;
    [SerializeField] private PlayerDataSO playerDataSO;
    [SerializeField] private GameTeamData teamDataSO;

    [ServerRpc(RequireOwnership = false)]
    public void AttemptToSpawnAgentsServerRpc(Vector3 position, Quaternion rotation, EUnitTypes type, ulong localPlayerID, int groupIndex, int baseId, FixedString32Bytes teamName, int damageStrength,
        int teamColorMaterialIndex, int health, NetworkObjectReference playerInfoObjectReference, int amountToSpawn, int cost)
    {
        int agentCountInGroup = playerDataSO.GetAIGroup(localPlayerID, groupIndex).agents.Count;
        int agentsToSpawn = AIGroup.MaxAgentsPerGroup - agentCountInGroup;

        // Don't spawn the agent if the group is full
        if(agentsToSpawn == 0)
        {
            playerInfoObjectReference.TryGet(out var playerInfoObject);
            PlayerInformation playerInformation = playerInfoObject.GetComponent<PlayerInformation>();

            // Refund the player the cost amount as no agents could be spawned
            playerInformation.IncreasePlayerCurrency(cost);

            return;
        }

        for(int i = 0; i < agentsToSpawn; ++i)
        {
            SpawnAgent(position, rotation, type, localPlayerID, groupIndex, baseId, teamName, damageStrength, teamColorMaterialIndex, health);
        }
    }

    private void SpawnAgent(Vector3 position, Quaternion rotation, EUnitTypes type, ulong localPlayerID, int groupIndex, int baseId, FixedString32Bytes teamName, int damageStrength,
        int teamColorMaterialIndex, int health)
    {
        //Debug.Log("Spawn AI agent [position: " + position + "] [rotation: " + rotation + "]");

        // Spawn agent
        GameObject agent = Instantiate(aiAgentPrefab, position, rotation);
        // Spawn agent on all client machines
        agent.GetComponent<NetworkObject>().Spawn();

        // Setup the agent
        AI_AgentController agentController = agent.GetComponent<AI_AgentController>();
        agentController.SetUnitType(type);
        agentController.SetEntityTeamName(teamName);
        agentController.SetDamageStrength(damageStrength);
        agentController.SetGroupIndex(groupIndex);
        agentController.SetAgentObjectReference(agent);
        agentController.SetHealth(health);

        // Spawn model prefab for the unit type parented to the agent
        AIUnitTypesData.UnitTypeInfo info = unitTypesData.GetUnitInfo(type);
        GameObject model = Instantiate(info.modelPrefab);
        model.transform.position = position;
        model.GetComponent<NetworkObject>().Spawn(); // Owned by the server 
        model.transform.SetParent(agent.transform);

        agentController.SetModelObjectReference(model);

        // Swap the model material to the team material
        // Assuming team 0 in the team data SO is the red team
        int teamIndex = (teamName.ConvertToString() == teamDataSO.GetTeamDataAtIndex(0).TeamName) ? 0 : 1;
        //SetAgentTeamColorClientRpc(model, teamIndex, teamColorMaterialIndex);

        // Add units to group in player data
        playerDataSO.AddAgentToGroup(localPlayerID, groupIndex, agent);

        BaseController selectedBaseController = null;
        BaseController[] baseControllers = FindObjectsOfType<BaseController>();
        foreach (BaseController baseController in baseControllers)
        {
            if (baseId == baseController.GetBaseId())
            {
                SetGroupUpLocationManagerClientRpc(agent.GetComponent<NetworkObject>().NetworkObjectId, baseId);
                selectedBaseController = baseController;
                break;
            }
        }

        playerDataSO.SetBaseControllerForGroup(localPlayerID, groupIndex, selectedBaseController);

        agent.GetComponent<AI_AgentController>().MoveToDefendPositionClientRpc(groupIndex, baseId, localPlayerID, false);

        //Debug.Log("Agents in group " + groupIndex + ": " + playerDataSO.GetAIGroup(localPlayerID, groupIndex).agents.Count);
    }

    [ClientRpc]
    public void SetGroupUpLocationManagerClientRpc(ulong localClientId, int baseId)
    {
        foreach(var unit in FindObjectsOfType<NetworkObject>())
        {
            if(unit.NetworkObjectId == localClientId)
            {
                foreach (BaseController baseController in FindObjectsOfType<BaseController>())
                {
                    if (baseId == baseController.GetBaseId())
                    {
                        unit.GetComponent<AI_AgentController>().groupUpLocationManager = baseController.m_meetUpLocationManager;
                    }
                }
            }
        }
    }

    [ClientRpc]
    public void SetAgentTeamColorClientRpc(NetworkObjectReference agentModelRef, int teamIndex, int materialIndex)
    {
        NetworkObject agentModelNetworkObject = null;
        agentModelRef.TryGet(out agentModelNetworkObject);

        Color teamColor = teamDataSO.GetTeamDataAtIndex(teamIndex).Colour;
        agentModelNetworkObject.GetComponent<Renderer>().materials[materialIndex].color = teamColor;
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnAgentDeathServerRpc(ulong localPlayerId, int groupIndex, NetworkObjectReference agentObjectReference)
    {
        NetworkObject agent = null;
        agentObjectReference.TryGet(out agent);

        playerDataSO.RemoveAgentFromGroup(localPlayerId, groupIndex, agent.gameObject);
    }
}
