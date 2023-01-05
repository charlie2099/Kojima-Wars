using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CS_HealingField : NetworkBehaviour
{
    public int healingRadius;
    public int healing;
    public int totalHealthHealed;
    public float totalTimeHealing;
    private float healingTimer;
    public GameObject player;
    public bool isBioGren = false;
    public CS_UseAbilities abilityScript;



    private void Start()
    {
        if (!IsServer)
        {

            GetComponent<Rigidbody>().isKinematic = true;
            GetComponentInChildren<BoxCollider>().enabled = false;
            enabled = false;
            return;
        }
        player = ServerAbilityManager.Instance.GetOwner(gameObject).gameObject;
        abilityScript = player.GetComponent<CS_UseAbilities>();
        if (abilityScript.bioticGren)
        {
            isBioGren = true;
        }
        else
        {
            GetComponentInChildren<BoxCollider>().enabled = false;
        }
        if (player == null)
        {
            Debug.LogError("NO PLAYER FOUND");
        }
       
    }

    void Update()
    {
        if(!isBioGren)
        {
            transform.position = player.transform.position;
        }
        totalTimeHealing -= Time.deltaTime;
        Collider[] objectsInRange = Physics.OverlapSphere(transform.position, healingRadius);
        foreach (var obj in objectsInRange)
        {
            Entity entityHit = obj.gameObject.GetComponent<Entity>();
            if (entityHit != null)
            {
                if (!ServerAbilityManager.Instance.IsEnemy(gameObject, entityHit))
                {
                    if (totalHealthHealed > 0 && obj.GetComponent<IDamageable>() != null)
                    {
                            var healthValue = HealthComponent.GetHealth(obj.gameObject.GetComponent<NetworkObject>().OwnerClientId);
                            if (healthValue < 100)
                            {
                                totalHealthHealed -= healing;
                                var value = healthValue + healing;
                                HealthComponent.SetHealth(obj.gameObject.GetComponent<NetworkObject>().OwnerClientId, value);
                            }
                    }
                }
            }
        }
        if (totalHealthHealed == 0 || totalTimeHealing <= 0)
        {
            if (isBioGren)
            {
                abilityScript.bioticGren = false;
                isBioGren = false;
            }
            Destroy(gameObject);
        }


    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, healingRadius);
    }

}
