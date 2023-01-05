using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CS_BioticGrenade : CS_NewGrenade
{ 

    [SerializeField] private float secondaryFuse;
    public GameObject healingRift;

        // Update is called once per frame
    void Update()
    {
       // fuse -= Time.deltaTime;
        secondaryFuse -= Time.deltaTime;
        if (fuse<=0 && secondaryFuse <=0 && exploded)
        {
            Destroy(gameObject);
        }
        if (exploded)
        {
            //Spawn healing aura
            abilityScript.bioticGren = true;
            GameObject healObj = Instantiate(healingRift, transform.position, Quaternion.identity);
            healObj.GetComponent<NetworkObject>().Spawn();
            ServerAbilityManager.Instance.RegisterNewAbilityServerRpc(new NetworkObjectReference(healObj), player);
            this.GetComponentInChildren<MeshRenderer>().enabled = false;
            secondaryFuse = 5;
            Destroy(gameObject);
        }
    }
}
