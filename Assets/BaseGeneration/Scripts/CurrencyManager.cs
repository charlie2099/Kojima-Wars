using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CurrencyManager : NetworkBehaviour
{
    public static event Action<int, string> OnCurrencyUpdate;
    
    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayerCurrencyServerRpc(int cash, string team)
    {
        UpdatePlayerCurrencyClientRpc(cash, team);
    }

    [ClientRpc]
    public void UpdatePlayerCurrencyClientRpc(int cash, string team)
    {
        if (OwnerClientId != NetworkManager.LocalClient.ClientId) return;
        OnCurrencyUpdate?.Invoke(cash, team);
    }
}
