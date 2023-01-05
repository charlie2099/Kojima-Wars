using UnityEngine;
using Unity.Netcode;
//using UnityEditor.Animations;

public class ActiveWeapon : NetworkBehaviour
{
    [SerializeField] private WeaponScript existingWeapon;
    [SerializeField] private Transform weaponParent;

    [SerializeField] private Transform weaponLeftGrip;
    [SerializeField] private Transform weaponRightGrip;

    public GameObject currentWeapon;
    Animator anim;
    AnimatorOverrideController overrides;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        overrides = anim.runtimeAnimatorController as AnimatorOverrideController;
       // overrides =  new AnimatorOverrideController(anim.runtimeAnimatorController);
        WeaponScript.OnWeaponEquipModel += OnWeaponEquipModel;
    }


    // Update is called once per frame

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Weapon equipped is" + existingWeapon.stats.weaponAnimation);
        }
    }

    public void OnWeaponEquipModel(ulong PlayerID)
    {
        OnWeaponEquipModelServerRpc(PlayerID);
    }
    [ServerRpc(RequireOwnership = false)]
    public void OnWeaponEquipModelServerRpc(ulong PlayerID)
    {
        OnWeaponEquipModelClientRpc(PlayerID);
    }

    [ClientRpc]
    public void OnWeaponEquipModelClientRpc(ulong PlayerID)
    {
        if(OwnerClientId != PlayerID)
        {
            return;
        }
 
        GameObject weaponModel = null;

        foreach(var obj in MechCharacterController.List)
        {
            if(obj.OwnerClientId == PlayerID)
            {
                var weapon = obj.GetComponent<CS_UseAbilities>().GetPlayerClass().GetWeapon();
                weaponModel = weapon.tpsWeaponModel;
                overrides["weapon_anim_empty"] = weapon.weaponAnimation;
                Debug.Log("Weapon equipped is" + weapon.weaponAnimation);
            }
        }
        if(weaponModel == null)
        {
            Debug.Log("Weapon model is equal to null!", this);
            return;
        }

        if (currentWeapon)
        {
            DestroyImmediate(currentWeapon.gameObject);
        }

        weaponModel = Instantiate(weaponModel);

        if (!IsOwner)
        {
            weaponModel.layer = LayerMask.NameToLayer("Player");
        }
        else
        {
            return;
        }

        weaponModel.transform.parent = weaponParent;
        weaponModel.transform.localPosition = Vector3.zero;
        weaponModel.transform.localRotation = Quaternion.identity;
        currentWeapon = weaponModel;
        anim.SetLayerWeight(1, 1.0f);
    }

    void SetAnimationDelayed()
    {
        overrides["weapon_anim_empty"] = existingWeapon.stats.weaponAnimation;
        Debug.Log("Weapon equipped is" + existingWeapon.stats.weaponAnimation);
    }

    //[ContextMenu("Save weapon pose")]
    //void SaveWeaponPose()
    //{
    //    GameObjectRecorder recorder = new GameObjectRecorder(gameObject);
    //    recorder.BindComponentsOfType<Transform>(weaponParent.gameObject, false);
    //    recorder.BindComponentsOfType<Transform>(weaponLeftGrip.gameObject, false);
    //    recorder.BindComponentsOfType<Transform>(weaponRightGrip.gameObject, false);
    //    recorder.TakeSnapshot(0.0f);
    //    recorder.SaveToClip(existingWeapon.stats.weaponAnimation);
    //    UnityEditor.AssetDatabase.SaveAssets();
    //}

    private void OnDestroy()
    {
        WeaponScript.OnWeaponEquipModel -= OnWeaponEquipModel;
    }
}
