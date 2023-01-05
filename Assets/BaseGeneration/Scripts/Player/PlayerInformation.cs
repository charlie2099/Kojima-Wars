using System;
using System.Collections;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerInformation : NetworkBehaviour
{
    public static event Action<PlayerInformation> OnMaxPlayerHealthSet;
    public static event Action<int, int> OnPlayerHealthUpdated;
    public static event Action<int, int> OnPlayerShieldsUpdated;
    public static event Action<string> OnPlayerNameSet;
    public static event Action<int> CurrencyUpdated;
    public static event Action<string> OnPlayerTeamSet;
    
    public string PlayerTeam { get; set; }
    public string PlayerName { get; set; }
    public int Currency { get; set; } = 50;

    public HealthComponent healthComponent { get; private set; }
    
    public void OnEnable()
    {
    }

    public void Start()
    {
        // Needs a slight delay for references to be set
        StartCoroutine(DelayedStartPlayerInformation());
    }

    private IEnumerator DelayedStartPlayerInformation()
    {
        // 0.5 seconds seems to be the correct amount before things dont get assigned properly
        yield return new WaitForSeconds(0.5f);
        if (OwnerClientId == NetworkManager.LocalClientId)
        {
            CurrencyManager.OnCurrencyUpdate += IncreasePlayerCurrencyIfNameMatch;

            // register callback to shields change value
            var hc = GetComponent<HealthComponent>();
            hc.GetCallback().AddListener(OnNetworkedHealthValueChanged);

            // register callback to health change value
            var sc = GetComponent<ShieldComponent>();
            sc.GetCallback().AddListener(OnNetworkedShieldsValueChanged);

            // Sets the initial values for the player information UI
            OnMaxPlayerHealthSet?.Invoke(this);
            OnPlayerTeamSet?.Invoke(PlayerTeam);
            OnPlayerNameSet?.Invoke(PlayerName);
        }
    }
    
    private static void OnNetworkedHealthValueChanged(int oldValue, int newValue)
    {
        OnPlayerHealthUpdated?.Invoke(oldValue, newValue);
    }
    
    private static void OnNetworkedShieldsValueChanged(int oldValue, int newValue)
    {
        OnPlayerShieldsUpdated?.Invoke(oldValue, newValue);
    }

    public void IncreasePlayerCurrencyIfNameMatch(int amount, string name)
    {
        if (name == PlayerTeam)
        {
            IncreasePlayerCurrency(amount);
        }
    }

    public void SetTeamName(string name)
    {
        PlayerTeam = name;
    }

    [ClientRpc]
    public void SetPlayerNameClientRpc(FixedString32Bytes newTeam)
    {
        PlayerName = newTeam.ConvertToString();
    }

    public void IncreasePlayerCurrency(int amount)
    {
        // cap the currency income at a certain amount
        // people who don't spend will only generate about 1400 a match anyway
        if (Currency + amount < 999)
        {
            Currency += amount;
            CurrencyUpdated?.Invoke(Currency);
        }
        else
        {
            // hard set
            Currency = 999;
            CurrencyUpdated?.Invoke(Currency);
        }
    }

    /*
    private void Update()
    {
        CheckCurrency();
    }

    private void CheckCurrency()
    {
        if (OwnerClientId != NetworkManager.LocalClient.ClientId) return;
        if (Currency > 999)
        {
            Currency = 999;
            CurrencyUpdated?.Invoke(Currency);
        }
    }
    */
    
    public void DecreasePlayerCurrency(int amount)
    {
        if (OwnerClientId != NetworkManager.LocalClient.ClientId) return;
        Currency -= amount;
        CurrencyUpdated?.Invoke(Currency);
    }
}
