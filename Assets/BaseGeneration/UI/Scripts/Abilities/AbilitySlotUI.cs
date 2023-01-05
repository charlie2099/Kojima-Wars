using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilitySlotUI : MonoBehaviour
{
    [SerializeField] private Image m_backImage;

    [Header("Ability References")] 
    [SerializeField] private GameObject m_abilityObject;
    [SerializeField] private GameObject m_cooldownObject;
    [SerializeField] private Image m_cooldownImage;
    [SerializeField] private Image m_slotKeyImage;
    [SerializeField] private TextMeshProUGUI m_cooldownText;

    [Header("Ability Settings")] 
    [SerializeField] private Color m_disabledColour;

    private bool m_OnCooldown;
    private Color m_defaultColour;
    private float m_cooldownTime;
    
    private void Start()
    {
        m_defaultColour = m_slotKeyImage.color;
        m_cooldownObject.SetActive(false);
        m_abilityObject.SetActive(true);
    }

    public bool IsOnCooldown()
    {
        return m_OnCooldown;
    }

    public float GetCooldownTime()
    {
        return m_cooldownTime;
    }


    public void SetAbilityImage(Sprite sprite)
    {
        if (sprite != null)
        {
            m_backImage.sprite = sprite;
            m_cooldownImage.sprite = sprite;

        }
        else
        {
            m_backImage.sprite = null;
            m_cooldownImage.sprite = null;
        }
    }

    public void SetIconColour(Color colour)
    {
        //m_fillColour.color = colour;
    }
    
    public void SetOnCoolDown(bool enabled, float time)
    {
        if (enabled)
        {
            m_OnCooldown = true;
            m_abilityObject.SetActive(false);
            m_cooldownObject.SetActive(true);

            m_slotKeyImage.color = m_disabledColour;
            m_cooldownTime = time;
        }
        else
        {
            m_OnCooldown = false;
            m_cooldownObject.SetActive(false);
            m_abilityObject.SetActive(true);

            m_slotKeyImage.color = m_defaultColour;
        }
    }

    public void SetCoolDownText(float time)
    {
        m_cooldownTime -= time;
        m_cooldownText.text = Mathf.FloorToInt(m_cooldownTime).ToString();
    }
}
