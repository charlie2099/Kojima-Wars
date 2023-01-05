using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerClassUI : MonoBehaviour
{
    public static PlayerClassUI Instance;

    //use this (need the actual scriptable object) to let the player know its confirmed class choice)
    public static event Action<int> OnClassAssigned = default;

    [Header("UI Containers")]
    [SerializeField] private List<GameObject> m_classContainers = new List<GameObject>();

    private PlayerClassSO m_selectedScriptable = default;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // set assault as our default class
        SwitchClassContainer(0);

        // Register on spawn callback
        Core.NetworkPlayerSetup nps = FindObjectOfType<Core.NetworkPlayerSetup>();
        nps.OnSpawnEvents += OnSpawn;

        InputManager.SetInputType(ControlType.NONE);
        CursorManager.EnableCursor("class-ui");
    }

    public void SwitchAltSet()
    {
        if (m_selectedScriptable.IsUsingAltAbilities())
        {
            m_selectedScriptable.SetUseAltAbility(false);
        }
        else
        {
            m_selectedScriptable.SetUseAltAbility(true);
        }
    }

    public void SwitchClassContainer(int i)
    {
        // hide all of our containers before setting 1 to visible
        HideClassContainers();

        // set the selected scriptable object dependent on the selection
        switch (i)
        {
            // assault
            case 0:
                m_classContainers[0].SetActive(true);
                m_selectedScriptable = m_classContainers[0].GetComponent<ClassSelectionUI>().GetClassScriptable();
                m_classContainers[0].GetComponent<ClassSelectionUI>().ResetAbilities();
                m_selectedScriptable.SetUseAltAbility(false);
                break;

            // defence
            case 1:
                m_classContainers[1].SetActive(true);
                m_selectedScriptable = m_classContainers[1].GetComponent<ClassSelectionUI>().GetClassScriptable();
                m_classContainers[1].GetComponent<ClassSelectionUI>().ResetAbilities();
                m_selectedScriptable.SetUseAltAbility(false);
                break;

            // movement
            case 2:
                m_classContainers[2].SetActive(true);
                m_selectedScriptable = m_classContainers[2].GetComponent<ClassSelectionUI>().GetClassScriptable();
                m_classContainers[2].GetComponent<ClassSelectionUI>().ResetAbilities();
                m_selectedScriptable.SetUseAltAbility(false);
                break;

            // recon
            case 3:
                m_classContainers[3].SetActive(true);
                m_selectedScriptable = m_classContainers[3].GetComponent<ClassSelectionUI>().GetClassScriptable();
                m_classContainers[3].GetComponent<ClassSelectionUI>().ResetAbilities();
                m_selectedScriptable.SetUseAltAbility(false);
                break;
        }
    }

    private void HideClassContainers()
    {
        foreach (var classContainer in m_classContainers)
        {
            classContainer.SetActive(false);
        }
    }

    public void ShowUI(bool show)
    {
        // hide this game object??
        this.gameObject.SetActive(show);
    }

    private void OnSpawn(ulong id)
    {
        InputManager.SetInputType(ControlType.NONE);
        CursorManager.EnableCursor("class-ui");
        ShowUI(true);
    }

    public void ClassConfirmed()
    {
        ShowUI(false);

        // Show the main HUD elements
        UIManager.Instance.FadeGroup(true, 0.25f, new List<UIGroupName>() { UIGroupName.GAME, UIGroupName.PLAYER });
        
        // Show the main HUD elements
        UIManager.Instance.FadeGroup(false, 0.25f, new List<UIGroupName>() { UIGroupName.SKILL});


        // send an event to whoever is subscribed?
        // pass through the scriptable object of the type that we have confirmed.
        OnClassAssigned?.Invoke(m_selectedScriptable.GetUniqueID());

        // Set the input back to mech. Assuming the player is always a mech when they are spawned
        InputManager.SetInputType(ControlType.MECH);
        CursorManager.DisableCursor("class-ui");

        // Enable damage after class selection
        var localClientID = NetworkManager.Singleton.LocalClientId;
        foreach(var cc in CombatComponent.List)
        {
            if (cc.OwnerClientId != localClientID) continue;
            cc.SetIsDead(false);    
        }
    }
}
