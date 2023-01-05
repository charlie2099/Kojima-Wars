using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(ShieldComponent))]
public class CombatComponent : NetworkBehaviour, IDamageable
{
    // maintains a static list of all health components
    public static List<CombatComponent> List = new List<CombatComponent>();
    public override void OnNetworkSpawn() => List.Add(this);
    public override void OnNetworkDespawn() => List.Remove(this);

    //UnitMap
    public int MaxHealthValue => MaxHealth;
    public int MaxShieldsValue => MaxShields;

    [SerializeField] private int MaxShields = 200;
    [SerializeField] private int MaxHealth = 100;

    [SerializeField] private UnityEvent OnSpawnEvent = default;

    public static Action<Entity> OnDeath = default;

    private const int MinValue = 0;
    private bool isDead = true;

    public bool IsDead() => isDead;
    public void HealDamage(int heal) => HealDamageServerRpc(heal);
    public void HealShield(int heal) => HealShieldServerRpc(heal);

    private bool inGodMode;
    public void TakeDamage(int damage) => TakeDamageServerRpc(damage);
    public void SetIsDead(bool value) => SetIsDeadServerRpc(value);

    [ServerRpc(RequireOwnership = false)]
    public void SetIsDeadServerRpc(bool value) => SetIsDeadClientRpc(value);

    [ClientRpc]
    public void SetIsDeadClientRpc(bool value)
    {
        //Debug.Log($"Set Alive {value} : ID = {OwnerClientId}");
        isDead = value;
    }

    public void Start()
    {
        if (!IsServer) return;

        HealthComponent.SetHealth(OwnerClientId, MaxHealth);
        ShieldComponent.SetShield(OwnerClientId, MaxShields);

        if(AudioManager.Instance != null)
        {
            AudioManager.Instance.CreateFMODInstance(AudioManager.Instance.events.uIAudioEvents.damageTaken, 1);
            AudioManager.Instance.CreateFMODInstance(AudioManager.Instance.events.playerAudioEvents.death, 1);
        }
    }

    /// <summary> 
    /// Called When the player is spawned
    /// </summary>
    public void OnSpawn(ulong id) => OnSpawnServerRpc(id);

    [ServerRpc(RequireOwnership = false)]
    public void OnSpawnServerRpc(ulong id)
    {
        OnSpawnClientRpc(id);

        HealthComponent.SetHealth(id, MaxHealth);
        ShieldComponent.SetShield(id, MaxShields);
    }

    [ClientRpc]
    public void OnSpawnClientRpc(ulong id)
    {
        OnSpawnEvent.Invoke();
    }

    private int DamageShield( int damage)
    {
        // don't deal damage if in god mode
        if (inGodMode) return 0;

        // dont do anything on ZERO, minus is ok
        if (damage == 0) return damage;

        // work out current health and clamp between values
        var shields = ShieldComponent.GetShield(OwnerClientId) - damage;

        // work out and set the overflow damage 
        damage = shields < 1 ? -shields : 0;

        // clamp values
        shields = Mathf.Clamp(shields, MinValue, MaxShields);

        // update the shield component
        ShieldComponent.SetShield(OwnerClientId, shields);
        return damage;
    }

    private int DamageHealth(int damage)
    {
        // don't deal damage if in god mode
        if (inGodMode) return 0;

        // dont do anything on ZERO, minus is ok
        if (damage == 0) return damage;

        // work out current health and clamp between values
        var health = HealthComponent.GetHealth(OwnerClientId) - damage;

        // work out and set the overflow damage 
        damage = health < 1 ? -health : 0;

        // clamp values
        health = Mathf.Clamp(health, MinValue, MaxShields);

        // update the health component
        HealthComponent.SetHealth(OwnerClientId, health);
        return damage;
    }

    /// <summary>
    /// interface for IDamageable
    /// </summary>
    /// 
    [ServerRpc(RequireOwnership = false)]
    public void HealDamageServerRpc(int heal)
    {
        // dont heal if already dead
        if (IsDead()) return;

        var newValue = HealthComponent.GetHealth(OwnerClientId);
        newValue = Mathf.Clamp(newValue, MinValue, MaxShields);
        HealthComponent.SetHealth(OwnerClientId, newValue);
    }

    [ServerRpc(RequireOwnership = false)]
    public void HealShieldServerRpc(int heal)
    {
        // dont heal if already dead
        if (IsDead()) return;

        var newValue = ShieldComponent.GetShield(OwnerClientId);
        newValue = Mathf.Clamp(newValue, MinValue, MaxShields);
        ShieldComponent.SetShield(OwnerClientId, newValue);
    }


    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        // dont kill if already dead
        if (IsDead()) return;
        // damage the shields first 
        var remainingAfterShield = DamageShield(damage);
        // carry remaining damage to health
        var remainingAfterHealth = DamageHealth(remainingAfterShield);

        // damage is remaining damage
        if(remainingAfterHealth > 0)
        {
            PlayerDeath();
        }

        if(OwnerClientId == NetworkManager.LocalClientId)
            AudioManager.Instance.PlayLocalFMODOneShot(AudioManager.Instance.events.uIAudioEvents.damageTaken, transform.position);
    }

    public bool IsAlive() => HealthComponent.GetHealth(OwnerClientId) > 0;
    public bool IsEnabled() => gameObject.activeInHierarchy;
    public bool IsRecalling() => gameObject.GetComponent<CS_UseAbilities>() != null ? gameObject.GetComponent<CS_UseAbilities>().recalling : false;
    /// Death stuff
    public void PlayerDeath() => PlayerDeathDeadServerRpc();

    [ServerRpc(RequireOwnership = false)]
    private void PlayerDeathDeadServerRpc() => PlayerDeadClientRpc();

    [ClientRpc]
    private void PlayerDeadClientRpc()
    {
        // hide the objects
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        // mark both mech and vtol as dead
        foreach (var cc in List)
        {
            if (cc.OwnerClientId != OwnerClientId) continue;
            cc.SetIsDead(true);

            // set the transform below the world
            //cc.transform.position = Vector3.down * 200;
        }

        // call the on death event 
        if (OwnerClientId == NetworkManager.LocalClientId)
        {
            OnDeath?.Invoke(GetComponent<Entity>());
            AudioManager.Instance.PlayLocalFMODOneShot(AudioManager.Instance.events.playerAudioEvents.death, transform.position);
        }
    }
    public void SetGod(bool newValue)
    {
        SetGodServerRpc(newValue);
    }

    [ServerRpc (RequireOwnership =false)]
    private void SetGodServerRpc(bool newValue)
    {
        SetGodClientRpc(newValue);
    }

    [ClientRpc]
    private void SetGodClientRpc(bool newValue)
    {
        inGodMode = newValue;

    }

    private void OnDestroy()
    {
        List.Clear();
    }
}


