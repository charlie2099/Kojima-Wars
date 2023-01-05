using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Networking;


public class CS_EMPBolt : NetworkBehaviour
{
    public float cast_time = 3f;
    public float cast_range = 200f;
    bool ready = true;
    public Material aim;
    public Material fire;
    private string current_material;
    [SerializeField] private float line_width;
    CS_Firepoints fire_points;
    GameObject player;
    LineRenderer lr;
    public LayerMask collidesWith;
    private Transform cam;
    // Start is called before the first frame update
    void Start()
    {
        if (!IsServer)
        {
            enabled = false;
            return;
        }
        current_material = "EMPAim";
        player = ServerAbilityManager.Instance.GetOwner(gameObject).gameObject;
        fire_points = player.GetComponent<CS_Firepoints>();
        lr = GetComponent<LineRenderer>();
        cam = player.GetComponentInChildren<MechLookPitch>().transform;
        lr.enabled = true;
        if (ready)
        {
            StartCoroutine(ShootTime());
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!IsServer) return;
        LaserPosServerRpc();

    }


    [ServerRpc]
    private void LaserPosServerRpc()
    {
        if (!ready || ready)
        {
            Vector3 target_pos = new Vector3();
            RaycastHit hit;
            Physics.Raycast(cam.position, cam.forward, out hit, cast_range, collidesWith, QueryTriggerInteraction.Ignore);
            if (hit.point.x != 0)
            {
                target_pos = hit.point;
                //Debug.Log(pos[1]);
            }
            else
            {
                target_pos = cam.position + new Vector3(0, 7, 0) + (cam.forward * cast_range);
                //Debug.Log(pos[1]);
            }
            //lr.SetPositions(pos);                       
            ServerAbilityManager.Instance.MatLineRenderServerRPC(GetComponent<NetworkObject>(), fire_points.shoulderWeaponFirepoint.position, target_pos, current_material, line_width);
        }
    }

    IEnumerator ShootTime()
    {
        //Debug.Log("Aiming bolt");
        line_width = 0.1f;
        current_material = "EMPAim";
        lr.enabled = true;
        ready = false;
        yield return new WaitForSeconds(cast_time - 0.3f);
        line_width = 1f;
        current_material = "EMPFire";
        yield return new WaitForSeconds(0.3f);
        FireBolt();
        //Debug.Log("Bolt fired");
        lr.material = aim;
        lr.enabled = false;
        ready = true;
        Destroy(this.gameObject);
    }

    void FireBolt()
    {
        RaycastHit hit;
        if (Physics.Raycast(player.GetComponentInChildren<MechLookPitch>().transform.position, player.GetComponentInChildren<MechLookPitch>().transform.forward, out hit, cast_range, collidesWith, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.gameObject.GetComponent<IDamageable>() != null)
            {
                if (hit.collider.gameObject.GetComponent<MechCharacterController>() == null)
                {
                    Debug.Log("Hit enemy Plane: " + hit.collider.name);
                    hit.collider.GetComponent<NetworkTransformComponent>().mode = Mode.Mech;
                    hit.collider.GetComponent<NetworkTransformComponent>().ForceSwitchMechMode();
                    ServerAbilityManager.Instance.DisableAbilitiesServerRPC(hit.collider.gameObject.GetComponent<NetworkObject>(), 10);
                    //other.gameObject.GetComponent<TransformController>().mode = 0; // turn any plane hit into mech
                }
                else
                {
                    Debug.Log("Hit");
                    Entity entityHit = hit.collider.gameObject.GetComponent<Entity>();
                    if (entityHit != null && ServerAbilityManager.Instance.IsEnemy(gameObject, entityHit))
                    {
                        ServerAbilityManager.Instance.DisableAbilitiesServerRPC(hit.collider.GetComponent<NetworkObject>(), 10);
                        Debug.Log("Hit " + entityHit.name);
                        //hit.collider.gameObject.GetComponent<TransformController>().mode = 0;   //should turn any plane hit into a mech?
                    }

                }

            }
            else
            {
                //Debug.Log("Missed");
            }
        }
        else
        {
            //Debug.Log("Missed");
        }
    }
}