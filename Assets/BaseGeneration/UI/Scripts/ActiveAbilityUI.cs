using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActiveAbilityUI : MonoBehaviour
{
    [SerializeField] private Image m_fillImage;
    [SerializeField] private Image m_iconImage;

    public void SetIconImage(Sprite icon)
    {
        m_iconImage.sprite = icon;
    }

    public Image GetFillImage()
    {
        return m_fillImage;
    }
}
