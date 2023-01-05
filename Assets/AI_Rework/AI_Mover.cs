using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI_Mover : MonoBehaviour
{

    public List<AIGroup> groups = new List<AIGroup>();

    public void MoveToInitialBasePos(AIGroup group, bool meetUp)
    {
        if (!groups.Contains(group))
        {
            groups.Add(group);
        }

        for (int i = 0; i < group.agents.Count; i++)
        {                
            AI_AgentController agent = group.agents[i].GetComponent<AI_AgentController>();
            agent.GetComponentInChildren<SphereCollider>().radius = 9;
            agent.SetLastDefenceLocationManager();
            agent.RemoveDefenceLocationInfo(agent.GetGroupIndex(), agent.newLocalPlayerId);
            agent.SetDefenceLocationManager(group.BaseController.GetBaseId());

            if (meetUp)
            {
                agent.meetUpMove = true;
                DefenceLocationInfo meetUpLocationInfo = group.groupUpLocationManager.defenceLocationInfo[i];

                if (agent.meetUpMove)
                {
                    agent.meetUpPos = meetUpLocationInfo.location;
                    agent.SetNavMeshAgentDestinationServerRpc(meetUpLocationInfo.location);
                    agent.meetUpMove = false;
                }
            }
            else
            {
                Debug.Log(group.state);
                if (group.state == AgentState.DEFENDING)
                {
                    MoveToDefenceLocation(group);
                }
                else
                {
                    agent.move = true;
                    for (int z = 0; z < 5; z++)
                    {
                        for (int x = 0; x < 5; x++)
                        {
                            DefenceLocationInfo locationInfo = group.BaseController.m_defenceLocationManager.unitDefenceLocations[x].defenceLocationInfo[z];

                            if (!locationInfo.occupied && agent.move)
                            {
                                agent.SetNavMeshAgentDestinationServerRpc(locationInfo.location);

                                agent.move = false;
                                break;
                            }

                        }
                    }
                }
            }
        }

        foreach(var unit in group.agents)
        {
            AI_AgentController agent = unit.GetComponent<AI_AgentController>();
            if (group.state == AgentState.ATTACKING)
            {
                agent.SetDefenceLocationInfoServerRpc(true);
            }
            else
            {
                agent.SetDefenceLocationInfoServerRpc(false);
            }
        }

        group.moveAttackState = AttackMovement.MOVE_TO_NEW_BASE_POS;
        group.moveDefenceState = DefenceMovement.MOVE_TO_NEW_BASE_POS;
    }

    public void MoveToNewBasePos(AIGroup group)
    {
        for (int i = 0; i < group.agents.Count; i++)
        {
            AI_AgentController agent = group.agents[i].GetComponent<AI_AgentController>();
            if (Vector3.Distance(agent.transform.position, agent.meetUpPos) > 5)
            {
                return;
            }
            if (i == group.agents.Count - 1)
            {
                DefenceLocationInfo locationInfo = AttackLocationInfo(group);

                StartCoroutine(MoveToInitialAttackPos(group, locationInfo));
                
                group.moveAttackState = AttackMovement.MOVE_TO_ATTACK_POS;
            }
        }
    }

    public DefenceLocationInfo AttackLocationInfo(AIGroup group)
    {
        for (int x = 0; x < 5; x++)
        {
            for (int z = 0; z < 5; z++)
            {
                DefenceLocationInfo locationInfo = group.BaseController.m_defenceLocationManager.unitDefenceLocations[x].defenceLocationInfo[z];
                if (locationInfo.occupied)
                {
                    return locationInfo;
                }
                if (x == 4 && z == 4)
                {
                    return locationInfo;
                }
            }
        }
        return null;

    }



    IEnumerator MoveToInitialAttackPos(AIGroup group, DefenceLocationInfo locationInfo)
    {
        foreach (var unit in group.agents)
        {
            AI_AgentController agent = unit.GetComponent<AI_AgentController>();

            Vector3 randomDirection = Random.insideUnitSphere * 15;
            randomDirection += locationInfo.location;
            NavMeshHit hit;
            NavMesh.SamplePosition(randomDirection, out hit, 15, NavMesh.AllAreas);

            agent.patrolPos = hit.position;

            agent.SetNavMeshAgentDestinationServerRpc(hit.position);
            agent.patrolPos = hit.position;
        }

        yield return new WaitForSeconds(1);

        group.moveAttackState = AttackMovement.MOVE_TO_ATTACK_POS;
    }


    public void MoveToAttackPos(AIGroup group)
    {
        foreach(var unit in group.agents)
        {
            AI_AgentController agent = unit.GetComponent<AI_AgentController>();

            if(agent.capturedBase)
            {
                group.state = AgentState.IDLE;
                group.moveAttackState = AttackMovement.MOVE_TO_MEET_UP_POS;
            }
        }
        for (int i = 0; i < group.agents.Count; i++)
        {
            AI_AgentController agent = group.agents[i].GetComponent<AI_AgentController>();
            if (Vector3.Distance(agent.transform.position, agent.patrolPos) > 15 &&
                agent.patrolPos != agent.previousPatrolPos)
            {
                return;
            }
            if (i == group.agents.Count - 1)
            {
                DefenceLocationInfo locationInfo = PatrolLocationInfo(group);

                MoveToNextAttackPos(group, locationInfo);
                Debug.Log("Test");
                return;
            }
        }
    }
    public DefenceLocationInfo PatrolLocationInfo(AIGroup group)
    {
        AI_AgentController agent = group.agents[0].GetComponent<AI_AgentController>();

        Debug.Log(agent.patrolIndex);

        DefenceLocationInfo locationInfo = agent.defenceLocationManager.unitDefenceLocations[agent.patrolIndex].defenceLocationInfo[2];

        return locationInfo;
    }

    public void MoveToNextAttackPos(AIGroup group, DefenceLocationInfo locationInfo)
    {
        foreach (var unit in group.agents)
        {
            AI_AgentController agent = unit.GetComponent<AI_AgentController>();

            agent.previousPatrolPos = agent.patrolPos;

            Vector3 randomDirection = Random.insideUnitSphere * 15;
            randomDirection += locationInfo.location;
            NavMeshHit hit;
            NavMesh.SamplePosition(randomDirection, out hit, 15, NavMesh.AllAreas);

            agent.patrolPos = hit.position;

            agent.SetNavMeshAgentDestinationServerRpc(hit.position);
            agent.patrolPos = hit.position;
            if (agent.patrolIndex == 4)
            {
                agent.patrolIndex = 0;
            }
            else
                agent.patrolIndex++;
        }
    }

    public void MoveToDefenceLocation(AIGroup group)
    {
        for (int i = 0; i < group.agents.Count; i++)
        {
            AI_AgentController agent = group.agents[i].GetComponent<AI_AgentController>();
            if (Vector3.Distance(agent.transform.position, agent.meetUpPos) > 5)
            {
                return;
            }
            if (i == group.agents.Count - 1)
            {
                MoveToNewDefenceLocation(group);

                group.moveDefenceState = DefenceMovement.AT_DEFENCE_POS;
            }
        }
    }

    public void MoveToNewDefenceLocation(AIGroup group)
    {
        foreach (var unit in group.agents)
        {
            AI_AgentController agent = unit.GetComponent<AI_AgentController>();
            agent.move = true;
            for (int z = 0; z < 5; z++)
            {
                for (int x = 0; x < 5; x++)
                {
                    DefenceLocationInfo locationInfo = group.BaseController.m_defenceLocationManager.unitDefenceLocations[x].defenceLocationInfo[z];

                    if (!locationInfo.occupied && agent.move)
                    {
                        Debug.Log(locationInfo.location);
                        agent.SetNavMeshAgentDestinationServerRpc(locationInfo.location);

                        locationInfo.occupied = true;

                        agent.move = false;
                    }
                }
            }
        }
    }

    void Update()
    {
        foreach (var group in groups)
        {
            foreach(var unit in group.agents)
            {
                if(unit == null)
                {
                    group.agents.Remove(unit);
                }
            }
            if (group.state == AgentState.ATTACKING)
            {
                switch (group.moveAttackState)
                {
                    case AttackMovement.MOVE_TO_MEET_UP_POS:
                        break;
                    case AttackMovement.MOVE_TO_NEW_BASE_POS:
                        MoveToNewBasePos(group);
                        break;
                    case AttackMovement.MOVE_TO_ATTACK_POS:
                        MoveToAttackPos(group);
                        break;
                    case AttackMovement.AT_ATTACK_POS:

                        break;
                }
            }
            if(group.state == AgentState.DEFENDING)
            {
                switch(group.moveDefenceState)
                {
                    case DefenceMovement.MOVE_TO_MEET_UP_POS:
                        break;
                    case DefenceMovement.MOVE_TO_NEW_BASE_POS:
                        MoveToDefenceLocation(group);
                        break;
                    case DefenceMovement.AT_DEFENCE_POS:
                        break;
                }
            }
        }
    }
}
