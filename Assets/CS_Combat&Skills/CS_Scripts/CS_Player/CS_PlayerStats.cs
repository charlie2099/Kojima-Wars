using Unity.Netcode;
using UnityEngine;

public class CS_PlayerStats : NetworkBehaviour
{
    [Header("Controls")]
    public float unscopedSensitivity = 20f;
    public float scopedSensitivity = 10f;
    public float currentSensitivity;


    [Header("Logic")]
    public float maxHealth = 100f;
    public float maxShields = 200f;
    public bool flashed = false;
    public bool hacked = false;
    public bool boostActive = false;
    public int damageBoost = 1;

    [Header("Video")]
    public int FOV = 60;

    [Header("Movement")]
    public float speed = 5f;
    public float acceleration = 1f;
    public float deceleration = 1f;
    public float airAccelerationMultiplier = 0.5f;
    public float maxRampAngle = 45f;
    public float jumpForce = 250f;
    public float gravity = -98.1f;
    public float maxStepSize = 1f;
    [Range(0,0.05F)] public float stepSmooth = 0f;

    public void Respawn()
    {
        speed = 25f;
        flashed = false;
        hacked = false;
        if (ServerAbilityManager.Instance.flashObj != null)
        {
            Destroy(ServerAbilityManager.Instance.flashObj);
        }
        if (ServerAbilityManager.Instance.timerUI != null)
        {
            Destroy(ServerAbilityManager.Instance.timerUI);
        }
        if (GetComponent<Entity>().TeamName == "red")
        {
            ServerAbilityManager.Instance.ChangeMaterialServerRpc(GetComponent<NetworkObject>(), "RedMaterial");
        }
        else
        {
            ServerAbilityManager.Instance.ChangeMaterialServerRpc(GetComponent<NetworkObject>(), "BlueMaterial");
        }
    }
}
