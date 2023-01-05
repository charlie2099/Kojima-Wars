using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class JetShoot : NetworkBehaviour
{
    LineRenderer line;

    [Header("Gun Settings")] 
    public float damage = 20.0F;
    public float spoolTime = 0.5f;
    public float fireRate = 0.2f;
    public float curFireRate = 0.0f;
    public float dispersalRadius = 0.1f;
    public float range = 5000.0f;
    public float overheatTime = 0;
    //public float coolDownTime = 1;
    public float normalSpread;
    bool isCoolingDown = false;

    public Transform gunPos;
    public float maxOverheatTime;
    float maxCooldownTime = 5;
    float lineTimer;
    bool startLineTimer;

    public Slider gunCooldownBar;
    public Image gunCooldownBarImage;

    // Start is called before the first frame update
    void Start()
    {
        line = gunPos.GetComponent<LineRenderer>();
        line.enabled = false;
        maxOverheatTime = overheatTime;
        //maxCooldownTime = coolDownTime;
        gunCooldownBar.maxValue = maxOverheatTime;

        if(AudioManager.Instance != null)
        {
            AudioManager.Instance.CreateFMODInstancePoolServerRpc(AudioManager.Instance.events.combatAudioEvents.minigunShot, 30, NetworkManager.Singleton.LocalClientId);
        }

    }
    
    // Update is called once per frame
    void Update()
    {
        if (line.enabled)
        {
            line.SetPosition(0, gunPos.position);
        }

        
        JetFiring();
        if (isCoolingDown)
        {
            overheatTime -= Time.deltaTime * 3;
            if (overheatTime <= 0)
            {
                overheatTime = 0;
                //coolDownTime = maxCooldownTime;
                isCoolingDown = false;
            }
        }

        if (startLineTimer)
        {
            lineTimer += Time.deltaTime;
            if(lineTimer >= 0.2f)
            {
                lineTimer = 0;
                line.enabled = false;
                startLineTimer = false;
            }
        }

    }



    void JetFiring()
    {
        if (IsOwner && GetComponent<VTOLCharacterController>().isJet)
        {
            if (InputManager.VTOL.Shoot.IsPressed())
            {
                
                
                overheatTime += Time.deltaTime;
                if (spoolTime > 0)
                {
                    spoolTime -= Time.deltaTime;
                }
                else
                {
                    if (overheatTime < 5 && !isCoolingDown)
                    {
                        if (curFireRate > 0)
                        {
                            curFireRate -= Time.deltaTime;
                        }
                        else
                        {
                            curFireRate = fireRate;
                            RaycastHit hit;
                            Vector3 dir = gunPos.transform.forward;
                            dir.x += Random.Range(-normalSpread, normalSpread);
                            dir.y += Random.Range(-normalSpread, normalSpread);
                            dir.z += Random.Range(-normalSpread, normalSpread);
                            
                            JetWeaponFiredServerRpc(gunPos.position,dir);

                            /*if (Physics.Raycast(gunPos.position, dir, out hit, range))
                            {
                                line.enabled = true;
                                //Debug.Log("Hit " + hit.collider.gameObject);
                                Debug.DrawRay(gunPos.position, dir * range, Color.red, 2);
                                line.SetPosition(1, hit.point);
                            }
                            else
                            {
                                line.enabled = true;
                                //Debug.Log("Miss");
                                Debug.DrawRay(gunPos.position, gunPos.transform.forward * range, Color.red, 2);
                                line.SetPosition(1, gunPos.transform.TransformDirection(Vector3.forward)+dir * range + gunPos.position);
                            }

                            AudioManager.Instance.PlayFMODOneShotServerRpc(AudioManager.Instance.events.combatAudioEvents.minigunShot, transform.position);*/

                        }
                    }
                    else
                    {
                        isCoolingDown = true;
                        line.enabled = false;
                    }
                }
            }
            else
            {
                isCoolingDown = true;
                line.enabled = false;
                if (spoolTime < 0.5)
                {
                    spoolTime += Time.deltaTime;
                }
            }
        }
        
    }

    [ServerRpc(RequireOwnership = false)]
    public void JetWeaponFiredServerRpc(Vector3 _pos, Vector3 _dir)
    {
        Vector3 endPoint = _pos + _dir * range;
        if (Physics.Raycast(_pos, _dir, out RaycastHit hit, range))
        {
            Entity attacker = GetComponent<Entity>();
            Entity target = hit.collider.gameObject.GetComponent<Entity>();
            if (hit.collider.gameObject.TryGetComponent(out IDamageable damageable))
            {
                // Check if this is friendly fire.
                if (target == null || attacker.TeamName != target.TeamName)
                {
                    damageable.TakeDamageServerRpc((int) damage);
                }
            }
        }
        JetWeaponFiredClientRpc(_pos, endPoint);
        AudioManager.Instance.PlayLocalFMODOneShot(AudioManager.Instance.events.combatAudioEvents.minigunShot, transform.position);
        AudioManager.Instance.PlayFMODOneShotServerRpc(AudioManager.Instance.events.combatAudioEvents.minigunShot, transform.position, NetworkManager.Singleton.LocalClientId, true);
    }

    [ClientRpc]
    public void JetWeaponFiredClientRpc(Vector3 _startPos, Vector3 _endPos)
    {
        startLineTimer = true;
        line.enabled = true;
        line.SetPosition(0,_startPos);
        line.SetPosition(1,_endPos);
    }
}
