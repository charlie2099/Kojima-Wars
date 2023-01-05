using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class CS_UseAbilities : NetworkBehaviour
{
    public static event Action OnAbility1CooldownMax;
    public static event Action OnAbility2CooldownMax;
    public static event Action OnAbility3CooldownMax;

    public enum Class
    {
        Damage,
        Movement,
        Defence,
        Recon
    }

    public Class playerClass = Class.Damage;

    [SerializeField] private PlayerClassSO playerClassSO = default;
    public PlayerClassSO GetPlayerClass() => playerClassSO;


    public GameObject model;
    public GameObject recallUIPrefab;
    private GameObject canvasObj;
    [SerializeField] private WeaponScript weaponScript;

    public float CooldownAbilityOne;
    public float CooldownAbilityTwo;
    public float CooldownAbilityThree;
    
    //public InputActionManager inputActions;
    public bool bioticGren;

    //Recall Ability (Don't delete without telling me please.)
    public float maxDuration = 10;
    public float saveInterval = 0.1f;
    public float recallSpeed = 50;
    public bool recalling { get; private set; }
    private List<Vector3> positions;
    private List<int> healths;
    private float saveStatsTimer;
    private float maxStatsStored;

    private PlayerClassUI classUI = default;
    private bool altSet = false;

    private void Start()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        PlayerClassUI.OnClassAssigned += SetClass;
        classUI = PlayerClassUI.Instance;

        InputManager.MECH.QAbility.started += OnAbilityOnePress;
        InputManager.MECH.EAbility.started += OnAbilityTwoPress;
        InputManager.MECH.FAbility.started += OnAbilityThreePress;

        maxStatsStored = maxDuration / saveInterval;
        positions = new List<Vector3>();
        healths = new List<int>();
    }

    public void SetClass(int classID) => SetClassServerRPC(OwnerClientId, classID);

    [ServerRpc(RequireOwnership = false)]
    private void SetClassServerRPC(ulong playerID, int classID) => SetClassClientRPC(playerID, classID);

    [ClientRpc]
    private void SetClassClientRPC(ulong playerID, int classID)
    {
        if (playerID != OwnerClientId) return;
        playerClassSO = PlayerClassSO.GetClassFromID(classID);
        weaponScript.AssignModel(playerClassSO.GetWeapon());
    }

    public void hack(NetworkObject id)
    {
        ulong id_ = id.OwnerClientId;
        if (id_ != NetworkManager.LocalClient.ClientId) return;
        
        var ability = playerClassSO.GetAbility(0);
        CooldownAbilityOne = ability.GetCooldownTime();
        OnAbility1CooldownMax?.Invoke();
        ability = playerClassSO.GetAbility(1);
        CooldownAbilityTwo = ability.GetCooldownTime();
        OnAbility2CooldownMax?.Invoke();
        ability = playerClassSO.GetAbility(2);
        CooldownAbilityThree = ability.GetCooldownTime();
        OnAbility3CooldownMax?.Invoke();


    }

    private void OnAbilityOnePress(InputAction.CallbackContext context)
    {
        var ability = playerClassSO.GetAbility(0);

        if (CooldownAbilityOne <= 0)
        {
            if (!recalling && positions.Count > 0 && playerClassSO.GetAbility(0).GetAbilityName() == "Recall")
            {
                recalling = true;
                canvasObj = Instantiate(recallUIPrefab, transform);
                GetComponent<MechCharacterController>().enabled = false;
                GetComponent<CapsuleCollider>().enabled = false;
                GetComponent<Rigidbody>().isKinematic = true;

                CooldownAbilityOne = ability.GetCooldownTime();
            }
            else
            {
                ability.CastAbility(GetComponent<Entity>(), 0);
                CooldownAbilityOne = ability.GetCooldownTime();
            }
        }

        if (CooldownAbilityOne >= ability.GetCooldownTime())
        {
            OnAbility1CooldownMax?.Invoke();
        }
    }

    private void OnAbilityTwoPress(InputAction.CallbackContext context)
    {
        var ability = playerClassSO.GetAbility(1);
        
        if (CooldownAbilityTwo <= 0)
        {
            ability.CastAbility(GetComponent<Entity>(), 1);
            CooldownAbilityTwo = ability.GetCooldownTime();
        }

        if (CooldownAbilityTwo >= ability.GetCooldownTime())
        {
            OnAbility2CooldownMax?.Invoke();
        }
    }

    private void OnAbilityThreePress(InputAction.CallbackContext context)
    {
        var ability = playerClassSO.GetAbility(2);
        if (CooldownAbilityThree <= 0)
        {
            ability.CastAbility(GetComponent<Entity>(), 2);
            CooldownAbilityThree = ability.GetCooldownTime();
        }

        if (CooldownAbilityThree >= ability.GetCooldownTime())
        {
            OnAbility3CooldownMax?.Invoke();
        }
    }

    private void Update()
    {
        var elapsed = Time.deltaTime;

        if (CooldownAbilityOne >= 0)
        {
            CooldownAbilityOne -= elapsed;
        }
        if (CooldownAbilityTwo>= 0)
        {
            CooldownAbilityTwo -= elapsed;
        }
        if (CooldownAbilityThree>= 0)
        {
            CooldownAbilityThree -= elapsed;
        }
        
        //Recall Ability (Don't delete without telling me please)
        if (playerClassSO != null && playerClassSO.GetAbility(0).GetAbilityName() == "Recall")
        {
            if (!recalling)
            {
                if (saveStatsTimer > 0)
                {
                    saveStatsTimer -= Time.deltaTime;
                }
                else
                {
                    StoreStats();
                }
            }
            else
            {
                if (positions.Count > 0)
                {
                    transform.position = Vector3.Lerp(transform.position, positions[0], recallSpeed * Time.deltaTime);
                    float dist = Vector3.Distance(transform.position, positions[0]);
                    if (dist < 0.25f)
                    {
                        SetStats();
                    }
                }
                else
                {
                    recalling = false;
                    Destroy(canvasObj);
                    GetComponent<MechCharacterController>().enabled = true;
                    GetComponent<CapsuleCollider>().enabled = true;
                    GetComponent<Rigidbody>().isKinematic = false;
                }
            }
        }
        
    }

    void StoreStats()
    {
        saveStatsTimer = saveInterval;
        positions.Insert(0, transform.position);
        healths.Insert(0, GetComponent<HealthComponent>().GetHealth());

        if (positions.Count > maxStatsStored)
        {
            positions.RemoveAt(positions.Count - 1);
        }
        if (healths.Count > maxStatsStored)
        {
            healths.RemoveAt(positions.Count - 1);
        }
    }

    void SetStats()
    {
        ServerAbilityManager.Instance.RecallHealthServerRPC(GetComponent<NetworkObject>(), healths[0]);
        transform.position = positions[0];
        positions.RemoveAt(0);
        healths.RemoveAt(0);
    }

    public void Respawn()
    {
        if (!IsOwner) return;
        positions.Clear();
        healths.Clear();
    }

    private void OnDestroy()
    {
        PlayerClassUI.OnClassAssigned -= SetClass;
        InputManager.MECH.QAbility.started -= OnAbilityOnePress;
        InputManager.MECH.EAbility.started -= OnAbilityTwoPress;
        InputManager.MECH.FAbility.started -= OnAbilityThreePress;
    }
}
