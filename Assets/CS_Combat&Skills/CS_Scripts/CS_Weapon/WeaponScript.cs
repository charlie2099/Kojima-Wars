using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using Unity.Netcode;
using Random = UnityEngine.Random;

public class WeaponScript : NetworkBehaviour
{
    public static event Action<ulong> OnWeaponEquipModel;
    public static event Action<WeaponStats> OnWeaponSet;
    public static event Action<int> OnBulletFired;
    public static event Action<WeaponScript> OnReloadFinished;

    public static event Action<bool> OnWeaponFired; 
    
    public int bulletsLeft, bulletsShot;

    [Header("------------References------------")]
    [Space(4)]
    public WeaponStats stats;
    public CinemachineVirtualCamera fpsCam;
    public Transform attackPoint;
    public RaycastHit rayHit;
    public LayerMask whatIsEnemy;
    public Animator animator;
    public GameObject recoil;
    public Recoil recoilScript;
    public Transform hipfirePos;
    public Transform adsPos;
    private GameObject weapon;
    public CS_PlayerStats playerStats;
    public MechLookPitch mlp;
    public GameObject player;
    public GameObject actualPlayer;
    public Rigidbody rb;
    public MechCharacterController mechCharacterController;

    [Header("------------GFX------------")]
    [Space(4)]
    public GameObject muzzleFlash;
    public GameObject bulletHoleGraphic;
    //public TextMeshProUGUI ammoText;
    //public TextMeshProUGUI nameText;
    //public GameObject HUD;
    public Image scopeSprite;
    public Image crosshair;

    //Bools
    private bool shooting, readyToShoot, reloading;
    public bool isFiring, isADS, infiniteAmmo, isRecoilOn;
    public float ADSAmount = 0.0F;

    [SerializeField] AppDataSO appdata;

    [SerializeField] WeaponStats[] weapons;

    private void Start()
    {
        StartCoroutine(DelayedStart());
        
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        //bulletsLeft = stats.magazineSize;
        readyToShoot = true;
        isRecoilOn = true;

        gameObject.layer = LayerMask.NameToLayer("Weapon");
        SetLayerRecursively(this.gameObject, "Weapon");
        
        // Delaying the start function to allow references to be assigned properly
        // this is a bit of a weird way to do it but you will have reference errors
        // if this is removed.
    }
    
    IEnumerator DelayedStart()
    {
        // this seems to be the sweet spot for things to catch up
        yield return new WaitForSeconds(0.3f);
        
        // set the UI for the current bullets left for this weapon
        if (OwnerClientId == NetworkManager.LocalClient.ClientId)
        {
            OnWeaponSet?.Invoke(stats);
        }
    }

    void SetLayerRecursively(GameObject obj, string layer)
    {
        obj.layer = LayerMask.NameToLayer(layer);
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    public void AssignFPSCam(CinemachineVirtualCamera virtualCamera)
    {
        fpsCam = virtualCamera;
    }

    private void OnEnable()
    {
        reloading = false;
        animator.SetBool("Reloading", false);
    }

    public void AssignModel(WeaponStats weaponStats)
    {
        if (OwnerClientId == NetworkManager.LocalClient.ClientId)
        {
            foreach (Transform child in this.gameObject.transform)
            {
                Destroy(child.gameObject);
            }
            weapon = Instantiate(weaponStats.fpsWeaponModel, Vector3.zero, Quaternion.identity, this.gameObject.transform);
            stats = weaponStats;
            weapon.layer = LayerMask.NameToLayer("Weapon");
            SetLayerRecursively(weapon, "Weapon");
            bulletsLeft = stats.magazineSize;
            OnWeaponEquipModel?.Invoke(OwnerClientId);
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.CreateFMODInstancePoolServerRpc(stats.shootSound, 30, NetworkManager.Singleton.LocalClientId);
                AudioManager.Instance.CreateFMODInstancePoolServerRpc(stats.reloadSound, 1, NetworkManager.Singleton.LocalClientId);
            }

            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.Euler(0, 180, 0);
            foreach (Transform child in weapon.transform)
            {
                if (child.tag == "FiringPoint")
                {
                    if (child.tag == "FiringPoint")
                    {
                        //AudioManager.Instance.CreateFMODInstancePoolServerRpc(stats.shootSound, 30);
                        // AudioManager.Instance.CreateFMODInstancePoolServerRpc(stats.reloadSound, 1);
                    }


                   

                    weapon.transform.localPosition = Vector3.zero;
                    weapon.transform.localRotation = Quaternion.Euler(0, 180, 0);

                    //foreach (Transform child in weapon.transform)
                    //{
                    //    if (child.tag == "FiringPoint")
                    //    {
                    //        attackPoint = child;
                    //    }

                    //}
                    //attackPoint = weapon.transform.GetChild(1);
                }
            }

            attackPoint = weapon.transform.GetChild(1);

            // set the UI for the current bullets left for this weapon
            if (OwnerClientId == NetworkManager.LocalClient.ClientId)
            {
                OnWeaponSet?.Invoke(stats);
            }
        }
  
    }
    private void Update()
    {
		isADS = stats.canADS && InputManager.MECH.ADS.IsPressed();
        shooting = stats.automatic ? InputManager.MECH.FireWeapon.IsPressed() : InputManager.MECH.FireWeapon.WasPressedThisFrame();
        
        // updates the UI with the current amount of bullets
        if (OwnerClientId == NetworkManager.LocalClient.ClientId)
        {
            OnWeaponFired?.Invoke(shooting);
          
        }
        
        isFiring = shooting && !reloading && bulletsLeft > 0;
        
        
        
        if (stats.automaticallyReload && bulletsLeft == 0 && !reloading) ReloadWeapon();

        //Shoot
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            bulletsShot = stats.firingMode == WeaponStats.FiringMode.Burst ? stats.bulletsPerBurst : 1;
            Shoot();
        }
        //Cancel Reload for single reloads (May want to keep for rifles too?)
        if (readyToShoot && shooting && reloading && stats.reloadType == WeaponStats.ReloadType.Single)
        {
            reloading = false;
        }

        ADSLogic();


        Transform MechCameraTransform = mlp.transform;
        float weaponDamage = stats.damage;

        Vector3 fwd = MechCameraTransform.forward * 1000.0F;
        Debug.DrawRay(player.transform.position, fwd, Color.red);

       // AudioManager.Instance.UpdatePosition(stats.shootSound, transform.position);
    }

    private void ADSLogic()
    {
        ADSAmount = Mathf.Clamp(ADSAmount + (isADS && !reloading ? 1.0F : -1.0F) * 1.0F / stats.ADSTime * Time.deltaTime, 0.0F, 1.0F);
        // Moving Weapon when ADS to look down sights
        transform.position = Vector3.Lerp(hipfirePos.position, adsPos.position, ADSAmount);
        transform.rotation = Quaternion.Slerp(hipfirePos.rotation, adsPos.rotation, ADSAmount);
        // Zooming to new FOV
        fpsCam.m_Lens.FieldOfView = Mathf.Lerp(appdata.fieldOfView, appdata.fieldOfView - stats.ADSZoom, ADSAmount);
    }

    public void Reload()
    {
        //Debug.Log("reload");

        if (bulletsLeft < stats.magazineSize && !reloading && !PauseManager.Instance.paused && !infiniteAmmo)
        {
            ReloadWeapon();
        }
    }

    private Vector3 GetFireDirection()
    {
        float spreadAmount = Random.Range(0.0F, Mathf.Lerp(stats.hipfireSpread, stats.ADSSpread, ADSAmount));
        float spreadAngle = Random.Range(0.0F, 360.0F);
        Vector3 fireDirection = Vector3.forward;
        fireDirection = Quaternion.AngleAxis(spreadAmount, Vector3.up) * fireDirection;
        fireDirection = Quaternion.AngleAxis(spreadAngle, Vector3.forward) * fireDirection;
        fireDirection = mlp.transform.rotation * fireDirection;
        return fireDirection;
    }

    private void Shoot()
    {
        readyToShoot = false;

        //Play Sound
        AudioManager.Instance.PlayLocalFMODOneShot(stats.shootSound, transform.position);
        AudioManager.Instance.PlayFMODOneShotServerRpc(stats.shootSound, transform.position, NetworkManager.Singleton.LocalClientId, true);

        Transform MechCameraTransform = mlp.transform;
        //float weaponDamage = stats.damage * playerStats.damageBoost;

        if (stats.firingMode == WeaponStats.FiringMode.Shotgun)
        {
            for (int i = 0; i < stats.numberOfPellets; i++)
            {
                ServerAbilityManager.Instance.WeaponFiredServerRpc(new NetworkObjectReference(mechCharacterController.gameObject),
                    MechCameraTransform.position, GetFireDirection());
            }
        }
        else
        {
            ServerAbilityManager.Instance.WeaponFiredServerRpc(new NetworkObjectReference(mechCharacterController.gameObject), MechCameraTransform.position, GetFireDirection());
            /*ServerAbilityManager.Instance.HandleMechWeaponFireServerRpc(player.transform.position, attackPoint.position,
                MechCameraTransform.forward, weaponDamage, gameObject.transform.root.GetComponent<NetworkObject>(),
                rb.gameObject.GetComponent<NetworkObject>(), stats.range);*/
        }

        if (isRecoilOn)
        {
            recoilScript.RecoilFire();
        }
        if (!infiniteAmmo)
        {
            bulletsLeft--; 
        }
        bulletsShot--;
        
        // updates the UI with the current amount of bullets
        if (OwnerClientId == NetworkManager.LocalClient.ClientId)
        {
            OnBulletFired?.Invoke(bulletsLeft);
        }

        if (!IsInvoking("ResetShot") && !readyToShoot)
        {
            Invoke("ResetShot", 1.0F / stats.fireRate);
        }
        if (bulletsShot > 0 && bulletsLeft > 0)
        {
            Invoke("Shoot", stats.timeBetweenBursts);
        }
        if (bulletsLeft <= 0)
        {
            Reload();
        }
    }

    private void ResetShot()
    {
        readyToShoot = true;
    }

    private void ReloadWeapon()
    {
        if (stats.reloadType == WeaponStats.ReloadType.Magazine)
        {
            AudioManager.Instance.PlayLocalFMODOneShot(stats.reloadSound, transform.position);

            animator.applyRootMotion = false;
            reloading = true;
            StartCoroutine(ReloadAnim());
            Invoke("ReloadFinished", stats.reloadTime);
        }
        else if (stats.reloadType == WeaponStats.ReloadType.Single)
        {
            StartCoroutine(ReloadSingle());
        }
        
    }

    IEnumerator ReloadSingle()
    {
        reloading = true;
        animator.applyRootMotion = false;
        while (reloading)
        {
            if (bulletsLeft < stats.magazineSize)
            {
                StartCoroutine(ReloadAnim());
                yield return new WaitForSeconds(stats.reloadTime);
                bulletsLeft++;
            }
            else
            {
                animator.applyRootMotion = true;
                reloading = false;
            }
        }
        
        if (OwnerClientId == NetworkManager.LocalClient.ClientId)
        {
            OnReloadFinished?.Invoke(this);
        }
    }

    private void ReloadFinished()
    {
        animator.applyRootMotion = true;
        bulletsLeft = stats.magazineSize;
        
        if (OwnerClientId == NetworkManager.LocalClient.ClientId)
        {
            OnReloadFinished?.Invoke(this);
        }
        
        reloading = false;
    }

    IEnumerator ReloadAnim()
    {
        animator.SetBool("Reloading", true);
        yield return new WaitForSeconds(stats.reloadTime - 0.25f);
        animator.SetBool("Reloading", false);
    }


}
