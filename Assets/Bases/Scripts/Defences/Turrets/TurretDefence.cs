using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TurretDefence : BaseDefence
{
    [Header("Turret Properties")]
    [SerializeField] protected ETurretDamageType m_fireType = ETurretDamageType.BASIC;
    [Tooltip("The delay in seconds between each firing of the turret.")]
    [SerializeField] protected float m_fireDelay = 0.0f;

    [SerializeField] private int damage = 10;

    [Header("Projectile References")]
    [SerializeField] protected Transform m_spawnPoint;
    [SerializeField] protected TurretProjectile m_projectilePrefab = null;

    [SerializeField] private GameObject muzzleFlashPrefab = null;

    [Header("Look At References")]
    [SerializeField] protected Transform m_lookAtTransform;
    [SerializeField] private bool m_lookAtXAxis;
    [SerializeField] private bool m_lookAtYAxis;
    [SerializeField] private bool m_lookAtZAxis;

    protected float m_fireTimer = 0.0f;
    private Vector3 fireDirection = Vector3.zero;
    private bool shouldFire = false;
    protected List<Entity> m_entitiesInRange = new List<Entity>();

    protected Entity m_target;

    private void Start()
    {
        if(AudioManager.Instance != null)
        {
            AudioManager.Instance.CreateFMODInstancePoolServerRpc(AudioManager.Instance.events.combatAudioEvents.turretFire, 6, NetworkManager.Singleton.LocalClientId);
        }
    }

    protected virtual void Update()
    {
        if (!IsServer) return;

        CheckTargetFire();

        ChooseTarget();

        UpdateTurretDefenceServerRpc();
    }

    protected virtual void ChooseTarget()
    {
        Entity closestEntity = null;
        float closestEntityDistance = 0;

        for (int i = 0; i < m_entitiesInRange.Count; i++)
        {
            Entity entity = m_entitiesInRange[i];

            if (entity.gameObject.active)
            {
                float distance = Vector3.Distance(m_lookAtTransform.position, entity.m_collider.bounds.center);

                RaycastHit hit;
                if (!Physics.Raycast(m_lookAtTransform.position, entity.m_collider.bounds.center - m_lookAtTransform.position, out hit, distance, LayerMask.GetMask("Ground")))
                {
                    if (closestEntity == null || distance < closestEntityDistance)
                    {
                        closestEntity = entity;
                        closestEntityDistance = distance;
                    }
                }
            }
            else
            {
                m_entitiesInRange.Remove(entity);
            }
        }

        m_target = closestEntity;
    }

    protected virtual void CheckTargetFire()
    {
        if (m_target != null)
        {
            LookToTarget();

            m_fireTimer += Time.deltaTime;
            if (m_fireTimer >= m_fireDelay)
            {
                m_fireTimer = 0;
                CheckFire();
            }
        }
    }

    [ServerRpc]
    protected virtual void UpdateTurretDefenceServerRpc()
    {
        UpdateTurretDefenceClientRpc(m_lookAtTransform.localRotation, shouldFire, fireDirection);
        if (shouldFire)
        {
            Fire(fireDirection);
        }
        shouldFire = false;
    }

    [ClientRpc]
    protected virtual void UpdateTurretDefenceClientRpc(Quaternion turretLookAtRotation, bool _shouldFire, Vector3 _fireDirection)
    {
        m_lookAtTransform.localRotation = turretLookAtRotation;
    }

    protected virtual void CheckFire()
    {
        fireDirection = m_target.m_collider.bounds.center - m_spawnPoint.position;
        shouldFire = true;
    }

    protected virtual void Fire(Vector3 _fireDirection)
    {
        AudioManager.Instance.PlayFMODOneShotServerRpc(AudioManager.Instance.events.combatAudioEvents.turretFire, transform.position, NetworkManager.Singleton.LocalClientId);

        RaycastHit hit;
        Physics.Raycast(m_spawnPoint.position, m_lookAtTransform.forward, out hit);
        if (hit.collider)
        {
            Entity hitOther = hit.collider.GetComponent<Entity>();
            if (hitOther != null)
            {
                if (hitOther.TeamName != m_baseController.TeamOwner)
                {
                    hitOther.GetComponent<IDamageable>().TakeDamageServerRpc(damage);
                    if (hitOther.GetComponent<IDamageable>().IsAlive() == false)
                    {
                        m_entitiesInRange.Remove(hitOther);
                    }
                }
            }
        }

        MuzzleFlashClientRpc();
    }

    protected virtual void LookToTarget()
    {
        var targetPosition = new Vector3(
            m_lookAtXAxis ? m_target.m_collider.bounds.center.x : transform.position.x,
            m_lookAtYAxis ? m_target.m_collider.bounds.center.y : transform.position.y,
            m_lookAtZAxis ? m_target.m_collider.bounds.center.z : transform.position.z
        );
        
        m_lookAtTransform.LookAt(targetPosition);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        Entity entity = other.gameObject.GetComponent<Entity>();

        if (entity == null)
        {
            return;
        }

        //if (entity.TeamName != m_baseController.TeamOwner)
        //{
            m_entitiesInRange.Add(entity);
        //}
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsServer)
            return;

        Entity entity = other.gameObject.GetComponent<Entity>();

        if (entity == null)
        {
            return;
        }

        if (!entity.gameObject.activeSelf)
        {
            m_entitiesInRange.Remove(entity);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        Entity entity = other.gameObject.GetComponent<Entity>();

        if (entity == null)
        {
            return;
        }

        m_entitiesInRange.Remove(entity);
    }

    [ClientRpc]
    private void MuzzleFlashClientRpc()
    {
        Instantiate(muzzleFlashPrefab, m_spawnPoint.position, m_spawnPoint.rotation);
    }
}