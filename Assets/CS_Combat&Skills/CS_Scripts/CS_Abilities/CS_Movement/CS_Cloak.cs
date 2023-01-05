using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class CS_Cloak : NetworkBehaviour
{
    private GameObject player;
    public float duration;
    public GameObject cloakUI;
    private GameObject _cloakUI;
    private float timer = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (!IsServer)
        {
            enabled = false;
            return;
        }
        player = ServerAbilityManager.Instance.GetOwner(gameObject).gameObject;
        //ServerAbilityManager.Instance.ShowUIClientRPC(player.GetComponent<NetworkObject>(), "Cloak", duration);
        StartCoroutine(Cloak(duration));
    }

    private void Update()
    {
        if (!IsServer) return;
        if (player.GetComponentInChildren<WeaponScript>() != null && player.GetComponentInChildren<WeaponScript>().isFiring)
        {
            if (player.GetComponent<Entity>().TeamName == "red")
            {
                ServerAbilityManager.Instance.ChangeMaterialServerRpc(player.GetComponent<NetworkObject>(),
                "RedMaterial");
            }
            else if (player.GetComponent<Entity>().TeamName == "blue")
            {
                ServerAbilityManager.Instance.ChangeMaterialServerRpc(player.GetComponent<NetworkObject>(),
                "BlueMaterial");
            }
            if (ServerAbilityManager.Instance.timerUI != null) Destroy(ServerAbilityManager.Instance.timerUI);
            Destroy(gameObject);
        }
    }

    IEnumerator Cloak(float timer)
    {
        // Add check for blue team once those materials get integrated
        if (player.GetComponent<Entity>().TeamName == "red")
        {
            ServerAbilityManager.Instance.ChangeMaterialServerRpc(player.GetComponent<NetworkObject>(),
            "RedCloak");
        }
        else if (player.GetComponent<Entity>().TeamName == "blue")
        {
            ServerAbilityManager.Instance.ChangeMaterialServerRpc(player.GetComponent<NetworkObject>(),
            "BlueCloak");
        }
        yield return new WaitForSeconds(timer);
        if (player.GetComponent<Entity>().TeamName == "red")
        {
            ServerAbilityManager.Instance.ChangeMaterialServerRpc(player.GetComponent<NetworkObject>(),
            "RedMaterial");
        }
        else if (player.GetComponent<Entity>().TeamName == "blue")
        {
            ServerAbilityManager.Instance.ChangeMaterialServerRpc(player.GetComponent<NetworkObject>(),
            "BlueMaterial");
        }

        if (ServerAbilityManager.Instance.timerUI != null) Destroy(ServerAbilityManager.Instance.timerUI);
        Destroy(gameObject);
    }
}
