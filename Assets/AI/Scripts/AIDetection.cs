using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class AIDetection : NetworkBehaviour
{
    [SerializeField]
    private float rotationSpeed = 5.0f;
    [SerializeField]
    private GameTeamData teamData;

    public bool has_target = false;
    public GameObject target = null;
    public float check_targets_timeout = 3; // how long between checking if the target is still in view/ range and then attack target
    float check_targets_timer = 0;

    public List<GameObject> possible_targets;
    public List<GameObject> in_range_targets;

    LayerMask ignoreMask;

    private SphereCollider detection_sphere = null;
    private AI_AgentController agentController = null;
    private Entity entityComponent = null;

    public override void OnNetworkSpawn()
    {
        detection_sphere = GetComponent<SphereCollider>();

        agentController = GetComponentInParent<AI_AgentController>();
        entityComponent = GetComponentInParent<Entity>();

        detection_sphere.isTrigger = true;

        possible_targets = new List<GameObject>();
        in_range_targets = new List<GameObject>();

        // Assuming team 0 in the team data SO is the red team
        ignoreMask = (entityComponent.TeamName == teamData.GetTeamDataAtIndex(0).TeamName) ? LayerMask.GetMask("AI_RED") : LayerMask.GetMask("AI_BLUE");

        setDetectionRadius(30);
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsHost) return;
        check_targets_timer += Time.deltaTime;

        if(in_range_targets.Count > 0 && target == null)
        {
            foreach(var unit in in_range_targets)
            {
                if(unit != null)
                {
                    target = unit;
                    has_target = true;
                    unit.GetComponent<AI_AgentController>().StopNavMeshAgentMovementServerRpc();
                }
                else
                {
                    //in_range_targets.Remove(unit);
                }
            }
        }

        if (has_target && target)
        {
            Vector3 dir = (target.transform.position - this.transform.position).normalized;
            dir.y = 0.0f;

            if (Vector3.Angle(transform.forward, dir) > 1.0f)
            {
                RotateToTarget(dir);
            }
        }
        if (check_targets_timer >= check_targets_timeout)
        {
            check_targets_timer = 0;
            if (in_range_targets.Count > 0)
            {
                possible_targets.Clear();
                List<GameObject> itemsToRemove;
                itemsToRemove = new List<GameObject>();
                foreach (var item in in_range_targets)
                {
                    if (item == null || item.GetComponent<IDamageable>().IsAlive() == false)
                    {
                        itemsToRemove.Add(item);
                        continue;
                    }
                    else
                    {
                        possible_targets.Add(item);
                    }
                }
                foreach (var item in itemsToRemove)
                {
                    in_range_targets.Remove(item);
                }

                if (has_target && target != null && possible_targets.Contains(target))
                {
                    attackTarget();

                }
                else
                {
                    pickTarget();
                    if (has_target)
                    {
                        attackTarget();
                    }
                }
            }
        }
        if (has_target)
        {
            agentController.StopNavMeshAgentMovementServerRpc();
        }
        else
        {
            agentController.ResumeNavMeshAgentMovementServerRpc();
        }
    }
    void attackTarget()
    {
        GameObject targetRef = target;
        //ServerAbilityManager.Instance.HandleMechWeaponFireServerRpc(transform.position, transform.position, dir, agentController.GetDamageStrength(),
        //    new Unity.Netcode.NetworkObjectReference(transform.parent.gameObject), new Unity.Netcode.NetworkObjectReference(transform.parent.gameObject));
        target.GetComponent<IDamageable>().TakeDamageServerRpc(agentController.GetDamageStrength());
        if (target.GetComponent<IDamageable>().IsAlive() == false)
        {
            //possible_targets.Remove(targetRef);
            //in_range_targets.Remove(targetRef);
            has_target = false;
            pickTarget();
        }
    }

    [ServerRpc]

    private void DestroyTargetServerRpc()
    {
        Destroy(target);
    }

    void RotateToTarget(Vector3 dir)
    {
        Vector3 lookTowards = Vector3.RotateTowards(transform.forward, dir, rotationSpeed * Time.deltaTime, 0.0f);
        transform.rotation = Quaternion.LookRotation(lookTowards, Vector3.up);
    }

    public void setDetectionRadius(float radii)
    {
        detection_sphere.radius = radii;
    }

    public float getDetectionRadius()
    {
        return detection_sphere.radius;
    }

    private void OnTriggerEnter(Collider other)
    {
        var otherEntityComponent = other.gameObject.GetComponent<Entity>();
        if (otherEntityComponent != null)
        {
            if (otherEntityComponent.TeamName != entityComponent.TeamName)
            {
                if (!in_range_targets.Contains(other.gameObject))
                {
                    in_range_targets.Add(other.gameObject);
                }


                if (!has_target)
                {
                    target = other.gameObject;
                    has_target = true;
                    agentController.StopNavMeshAgentMovementServerRpc();
                }
            }

        }
    }


    private void pickTarget()
    {
        if (possible_targets.Count > 0)
        {
            target = possible_targets[Random.Range(0, possible_targets.Count - 1)];
            has_target = true;
        }
        else
        {
            target = null;
            has_target = false;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        var script = other.gameObject.GetComponent<Entity>();

        if (script != null)
        {
            if (script.TeamName != GetComponentInParent<Entity>().TeamName)
            {
                if (in_range_targets.Contains(other.gameObject))
                {
                    in_range_targets.Remove(other.gameObject);
                }
                if (possible_targets.Contains(other.gameObject))
                {
                    possible_targets.Remove(other.gameObject);
                }
                if (has_target)
                {
                    if (other.gameObject == target)
                    {
                        if (possible_targets.Count > 0)
                        {
                            pickTarget();
                        }
                        else
                            target = null;
                    }
                }
            }
        }
    }

}
