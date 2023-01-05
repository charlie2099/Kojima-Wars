using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;

public class CS_GroundSlam : NetworkBehaviour
{
    [SerializeField] private float speedModifier;
    [SerializeField] private float stunTime;

    public float radius;
    [Range(0, 360)] public float viewAngle;

    //TODO TO be replaced when isEnemy is implemented 
    //public GameObject enemy;
    private GameObject player;

    private CS_Firepoints groundSlamPoint;
   // public GameObject explosion;
    
    //Layer of enemies
   // public LayerMask enemyMask;

    //Layer for walls etc
    public LayerMask obstructionMask;
    
    public int damage;

    // Start is called before the first frame update
    void Start()
    {
        if (!IsServer)
        {
            enabled = false;
            return;
        }
        player = ServerAbilityManager.Instance.GetOwner(gameObject).gameObject;
        groundSlamPoint = player.GetComponent<CS_Firepoints>();
        transform.position = groundSlamPoint.groundSlamPoint.position;
        transform.rotation = groundSlamPoint.groundSlamPoint.rotation;
 
       
   
        var position = groundSlamPoint.groundSlamPoint.position;

        var rotation = Quaternion.LookRotation
            (player.GetComponentInChildren<MechLookPitch>().transform.forward);
        
      //  GameObject explosionObj = Instantiate(explosion, position, rotation);
     //   explosionObj.GetComponent<NetworkObject>().Spawn();
        
        if(AudioManager.Instance != null)
        {
            AudioManager.Instance.CreateFMODInstancePoolServerRpc(AudioManager.Instance.events.combatAudioEvents.slam, 4, NetworkManager.Singleton.LocalClientId);
        }
        
        GroundSlam();
    }

    // Update is called once per frame
    void Update()
    {
        AudioManager.Instance.UpdatePosition(AudioManager.Instance.events.combatAudioEvents.slam, player.transform.position);
    }

    private void GroundSlam()
    {
        AudioManager.Instance.PlayFMODOneShotServerRpc(AudioManager.Instance.events.combatAudioEvents.slam, player.transform.position, NetworkManager.Singleton.LocalClientId);

        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, radius);

        foreach (var obj in enemiesInRange)
        {
            if (obj.GetComponent<IDamageable>() != null && enemiesInRange.Length != 0)
            {
                Transform targets = obj.transform;
                Vector3 directionFromTarget = obj.transform.position - player.transform.position.normalized;
                
                Entity entityHit = obj.GetComponent<Entity>();
                Debug.Log("Entity hit: " + entityHit.name);
              
                if (Vector3.Angle(player.transform.forward, directionFromTarget) < viewAngle / 2)
                {
                    float distanceFromTarget = Vector3.Distance(obj.transform.position, player.transform.position);
                    
                    //Check to see if enemy in frustum of player, obstruction mask to stop it working through walls 
                    if (!Physics.Raycast(player.transform.position, directionFromTarget, distanceFromTarget,
                        obstructionMask))
                    {
                        Debug.Log("Player Hit");
                        if (obj.GetComponent<IDamageable>() != null)
                        {
                            DamageEnemies(obj);
                            StunPlayers(obj);
                        }
                    }
                    else
                    {
                       Destroy(gameObject,5);
                       Debug.Log("Player Missed");
                    }
                }
                else
                {
                    Debug.Log("Player Missed");
                    Destroy(gameObject, 5);
                }
            }
        }
    }

    private void DamageEnemies(Collider targets)
    {
        Entity entityHit = targets.GetComponent<Entity>();
        if (ServerAbilityManager.Instance.IsEnemy(gameObject, entityHit))
        {
            
            entityHit.GetComponent<IDamageable>().TakeDamageServerRpc(damage);
            
        }
    }

    private void StunPlayers(Collider targets)
    {
        Entity entityHit = targets.GetComponent<Entity>();
        if (ServerAbilityManager.Instance.IsEnemy(gameObject, entityHit))
        {
            ServerAbilityManager.Instance.PlayerStatusServerRPC(entityHit.gameObject, "Slow", 3);
            entityHit.GetComponent<IDamageable>().TakeDamageServerRpc(damage);
            Debug.Log(entityHit + "Slowed/Stunner");
            Debug.Log(damage + "- Damage");
            Destroy(gameObject);
        }
        
    }
    
}