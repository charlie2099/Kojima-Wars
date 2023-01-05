using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilitiesUI : MonoBehaviour
{
    [Header("Ability Slots")]
    [SerializeField] private List<AbilitySlotUI> slots = new List<AbilitySlotUI>(3);

    [Header("Active Ability Settings")] 
    [SerializeField] private GameObject m_activeParentObject;
    [SerializeField] private GameObject m_activeAbilityObject;
    [SerializeField] private ActiveAbilityHandler m_abilityHandler;
    
    private PlayerClassSO m_playerClassSo;
    
    private bool altSet = false;

    private void OnEnable()
    {
        PlayerClassUI.OnClassAssigned += OnClassAssigned;
        CS_UseAbilities.OnAbility1CooldownMax += OnCooldown1Max;
        CS_UseAbilities.OnAbility2CooldownMax += OnCooldown2Max;
        CS_UseAbilities.OnAbility3CooldownMax += OnCooldown3Max;

    }
    
    private void OnDisable()
    {
        PlayerClassUI.OnClassAssigned -= OnClassAssigned;
        CS_UseAbilities.OnAbility1CooldownMax -= OnCooldown1Max;
        CS_UseAbilities.OnAbility2CooldownMax -= OnCooldown2Max;
        CS_UseAbilities.OnAbility3CooldownMax -= OnCooldown3Max;
    }
    
    private void OnClassAssigned(int classID)
    {
        // just setting the background image for now (will need to pass in the ability so in future)
        // (for the cooldown of each ability)

        var so = PlayerClassSO.GetClassFromID(classID);
        m_playerClassSo = PlayerClassSO.GetClassFromID(classID);

        altSet = so.IsUsingAltAbilities();
        for (var i = 0; i < 3; i++)
        {
            var slot = slots[i];
            var ability = so.GetAbility(i);
            slot.SetAbilityImage(ability.GetIconSprite());
            slot.SetIconColour(so.GetClassColour());
        }
    }

    private void Update()
    {
        for (var i = 0; i < 3; i++)
        {
            var slot = slots[i];
            if (slot.IsOnCooldown())
            {
                slot.SetCoolDownText(Time.deltaTime);

                if (slot.GetCooldownTime() < 0)
                {
                    slot.SetOnCoolDown(false, 0);
                }
            }
            else
            {
                slot.SetOnCoolDown(false, 0);
            }
        }
    }

    // just for testing whilst i can't play from main menu
    [ContextMenu("USE SOMETHING")]
    public void Something()
    {
        OnCooldown1Max();
        OnCooldown2Max();
        OnCooldown3Max();
    }

    public void OnCooldown1Max()
    {
        var slot = slots[0];
        slot.SetOnCoolDown(true, m_playerClassSo.GetAbility(0).GetCooldownTime());
        SpawnActiveAbility(0);
    }

    public void OnCooldown2Max()
    {
        var slot = slots[1];
        slot.SetOnCoolDown(true, m_playerClassSo.GetAbility(1).GetCooldownTime());
        SpawnActiveAbility(1);
    }

    public void OnCooldown3Max()
    {
        var slot = slots[2];
        slot.SetOnCoolDown(true, m_playerClassSo.GetAbility(2).GetCooldownTime());
        SpawnActiveAbility(2);
    }

    public void SpawnActiveAbility(int index)
    {
        if (m_playerClassSo.GetAbility(index).GetActiveTime() > 0)
        {
            var go = Instantiate(m_activeAbilityObject, m_activeParentObject.transform);
            m_abilityHandler.StartActiveAbility(m_playerClassSo.GetAbility(index), go);
        }
    }
    
}
