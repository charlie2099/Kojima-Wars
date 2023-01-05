using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SocialPlatforms;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class CS_PlasmaBeam : NetworkBehaviour
{
    //Customisation
    [SerializeField] private float length;
    [SerializeField] private float duration;
    [SerializeField] private int damage;
    [SerializeField] private GameObject explosion;
    [SerializeField] private float damageRadius;
    [SerializeField] private LayerMask collidesWith;

    bool exploded = false;

    
    //References
    private LineRenderer lr;
    private GameObject player;
    private CS_Firepoints firepoint;
    private Transform cameraTransform;
    public Vector3 offset;


    
    void Start()
    {
        if (!IsServer)
        {
            enabled = false;
            return;
        }
        
        //Get references
        player = ServerAbilityManager.Instance.GetOwner(gameObject).gameObject;
        firepoint = player.GetComponent<CS_Firepoints>();
        lr = GetComponent<LineRenderer>();
        cameraTransform = player.GetComponentInChildren<MechLookPitch>().transform;
        
        lr.positionCount = 2;

        if(AudioManager.Instance != null)
        {
            AudioManager.Instance.CreateFMODInstancePoolServerRpc(AudioManager.Instance.events.combatAudioEvents.plasma, 1, NetworkManager.Singleton.LocalClientId);
            AudioManager.Instance.CreateFMODInstancePoolServerRpc(AudioManager.Instance.events.combatAudioEvents.plasmaCharge, 1, NetworkManager.Singleton.LocalClientId);
        }
    }
    
   

    void FixedUpdate()
    {
        if (!IsServer) return;
        //Count down fuse
        duration -= Time.deltaTime;
        if (duration <= 0)
        {
            Destroy(gameObject);
        }
        
        beamPositionServerRpc();
    }
    
    [ServerRpc]
    private void beamPositionServerRpc()
    {
        //Make the beam come from the shoulder mounted weapon
        //lr.SetPosition(0, firepoint.shoulderWeaponFirepoint.position);

        AudioManager.Instance.PlayFMODOneShotServerRpc(AudioManager.Instance.events.combatAudioEvents.plasmaCharge, player.transform.position, NetworkManager.Singleton.LocalClientId);
        AudioManager.Instance.UpdatePosition(AudioManager.Instance.events.combatAudioEvents.plasmaCharge, player.transform.position, NetworkManager.Singleton.LocalClientId);

        RaycastHit hit;
        Vector3 targetPoint;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, length, collidesWith, QueryTriggerInteraction.Ignore))
        {
            targetPoint = hit.point;
        }
        else
        {
            Vector3 direction = cameraTransform.forward * length;
            targetPoint = firepoint.transform.position + offset + direction;
        }
        //lr.SetPosition(1, targetPoint);

        ServerAbilityManager.Instance.LineRenderServerRPC(GetComponent<NetworkObject>(),
            firepoint.shoulderWeaponFirepoint.position, targetPoint);

        //Explode at the end
        if (duration <= 0.01 && !exploded)
        {
            SpawnParticleEffectClientRpc(targetPoint, Quaternion.identity);

            AudioManager.Instance.PlayFMODOneShotServerRpc(AudioManager.Instance.events.combatAudioEvents.plasma, player.transform.position, NetworkManager.Singleton.LocalClientId);

            exploded = true;

            Collider[] EnemiesToDamage = Physics.OverlapSphere(targetPoint, damageRadius);

            foreach (var obj in EnemiesToDamage)
            {
                //Damage enemy
                if(obj.GetComponent<IDamageable>() != null)
                {
                    Entity entityHit = obj.GetComponent<Entity>();
                    if(ServerAbilityManager.Instance.IsEnemy(gameObject, entityHit))
                    {
                        obj.GetComponent<IDamageable>().TakeDamageServerRpc(damage);
                    }
                }
            }
            

        }
    }

    [ClientRpc]
    private void SpawnParticleEffectClientRpc(Vector3 position, Quaternion rotation)
    {
        // Executed on all connected clients
        // Spawn the particle effect object
        Instantiate(explosion, position, rotation);
    }
    
}
