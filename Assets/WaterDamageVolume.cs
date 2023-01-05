using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class WaterDamageVolume : NetworkBehaviour
{
    [SerializeField] private float damageFrequencyInSeconds = default;
    [SerializeField] private int damagePerTick = default;

    [SerializeField] private List<IDamageable> _list = new List<IDamageable>();
    private List<IDamageable> _removeList = new List<IDamageable>();
    private float timer = default;

    private void Start()
    {
        if (IsServer) return;
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        // return if object is not damageable
        if (!other.TryGetComponent<IDamageable>(out var damageable)) return;
        // add it to the list 
        _list.Add(damageable);

        if (other.GetComponentInChildren<PlayerAudioManager>() != null)
            other.GetComponentInChildren<PlayerAudioManager>().inWater = true;
    }

    private void OnTriggerExit(Collider other)
    {
        // return if object is not damageable
        if (!other.TryGetComponent<IDamageable>(out var damageable)) return;
        // return if it somehow the object left but did not enter
        if (!_list.Contains(damageable)) return;
        // ad it to the list 
        _list.Remove(damageable);

        if(other.GetComponentInChildren<PlayerAudioManager>() != null)
            other.GetComponentInChildren<PlayerAudioManager>().inWater = false;
    }

    private void FixedUpdate()
    {
        ApplyFixedRateDamageServerRpc();
    }

    [ServerRpc(RequireOwnership = true)]
    private void ApplyFixedRateDamageServerRpc()
    {
        // increment the timer 
        timer -= Time.deltaTime;
        if (timer > 0) return;

        // apply damage to damageables
        foreach (var obj in _list)
        {
            if (!obj.IsAlive() || !obj.IsEnabled()) _removeList.Add(obj);
            if (obj.IsRecalling() && obj.IsEnabled()) _removeList.Add(obj);
            obj.TakeDamageServerRpc(damagePerTick);
        }

        // remove dead objects
        foreach(var obj in _removeList) _list.Remove(obj);
        _removeList.Clear();
        
        // reset timer
        timer = damageFrequencyInSeconds;
    }

}
