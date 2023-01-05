using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClassSelectionUI : MonoBehaviour
{
    [Header("Scriptable")] 
    [SerializeField] private PlayerClassSO m_classScriptable;

    [Header("Ability UI Elements")]
    [SerializeField] private List<Image> m_abilityImages;
    [SerializeField] private List<TextMeshProUGUI> m_abilityText;
    [SerializeField] private List<TextMeshProUGUI> m_abilityCooldown;
    [SerializeField] private List<TextMeshProUGUI> m_abilityDescription;

    [Header("Weapon UI Elements")] 
    [SerializeField] private Image m_weaponImage;
    [SerializeField] private TextMeshProUGUI m_weaponName;
    [SerializeField] private TextMeshProUGUI m_weaponDescription;
    
    [SerializeField] private List<TextMeshProUGUI> m_statName;
    [SerializeField] private List<TextMeshProUGUI> m_statAmount;

    private bool altAbilities = false;

    // Start is called before the first frame update
    void Start()
    {
        SetAbilityElements();
        SetWeaponElements();
    }

    public PlayerClassSO GetClassScriptable()
    {
        return m_classScriptable;
    }

    private void SetAbilityElements()
    {
        for (int i = 0; i < 3; i++)
        {
            m_abilityImages[i].sprite = m_classScriptable.GetAbilityForInfo(i).GetIconSprite();
            m_abilityText[i].text = m_classScriptable.GetAbilityForInfo(i).GetAbilityName();
            m_abilityCooldown[i].text = m_classScriptable.GetAbilityForInfo(i).GetCooldownTime().ToString("#");
            m_abilityDescription[i].text = m_classScriptable.GetAbilityForInfo(i).GetAbilityDescription();
        }
    }
    private void SetAltAbilityElements()
    {
        for (int i = 0; i < 3; i++)
        {
            m_abilityImages[i].sprite = m_classScriptable.GetAltAbilityForInfo(i).GetIconSprite();
            m_abilityText[i].text = m_classScriptable.GetAltAbilityForInfo(i).GetAbilityName();
            m_abilityCooldown[i].text = m_classScriptable.GetAltAbilityForInfo(i).GetCooldownTime().ToString("#");
            m_abilityDescription[i].text = m_classScriptable.GetAltAbilityForInfo(i).GetAbilityDescription();
        }
    }

    public void SwitchAbilities()
    {
        if (altAbilities)
        {
            SetAbilityElements();
            altAbilities = false;
        }
        else
        {
            SetAltAbilityElements();
            altAbilities = true;
        }
    }

    public void ResetAbilities()
    {
        SetAbilityElements();
        altAbilities = false;
    }

    private void SetWeaponElements()
    {
        m_weaponImage.sprite = m_classScriptable.GetWeapon().icon;
        m_weaponName.text = m_classScriptable.GetWeapon().weaponName;
        
        // need a description in the weapon object
        m_weaponDescription.text = m_classScriptable.GetWeapon().weaponDescription;
        
        m_statName[0].text = "Damage";
        m_statAmount[0].text = m_classScriptable.GetWeapon().damage.ToString("#.#");
        
        m_statName[1].text = "Magazine";
        m_statAmount[1].text = m_classScriptable.GetWeapon().magazineSize.ToString("#.#");
        
        m_statName[2].text = "Reload";
        if (m_classScriptable.GetWeapon().reloadType == WeaponStats.ReloadType.Single)
        {
            float reloadTime = m_classScriptable.GetWeapon().reloadTime * m_classScriptable.GetWeapon().magazineSize;
            m_statAmount[2].text = reloadTime.ToString("#.#");
        }
        else
        {
            m_statAmount[2].text = m_classScriptable.GetWeapon().reloadTime.ToString("#.#");
        }
        
    }
}
