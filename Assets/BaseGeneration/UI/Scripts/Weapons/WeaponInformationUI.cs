using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponInformationUI : MonoBehaviour
{
    [SerializeField] private Image m_weaponImage;
    [SerializeField] private TextMeshProUGUI m_currentAmmoText;
    [SerializeField] private TextMeshProUGUI m_maxAmmoText;

    [Header("Reload Notification Settings")] [SerializeField]
    private TextMeshProUGUI m_reloadText;

    [SerializeField] private CanvasGroup m_reloadObject;
    [SerializeField] private Color m_defaultColour = Color.white;
    [SerializeField] private Color m_highlightColour = Color.yellow;

    private WeaponStats m_stats;
    private bool m_blink;

    private Sprite m_weaponSprite;
    private int m_magazineSize;
    private int m_currentMagazine;

    // Start is called before the first frame update
    void Start()
    {
        // Delaying the start function to allow references to be assigned properly
        // this is a bit of a weird way to do it but you will have reference errors
        // if this is removed.
        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        // this seems to be the sweet spot for things to catch up
        yield return new WaitForSeconds(0.1f);

        // assign to weapon event after the weapon has been set up
        WeaponScript.OnWeaponSet += OnWeaponSet;
        WeaponScript.OnBulletFired += OnCurrentBulletsChanged;
        WeaponScript.OnReloadFinished += OnReloadFinished;
        
        m_reloadText.text = " ";
        m_reloadObject.alpha = 0;
        m_blink = false;
    }

    private void OnDisable()
    {
        WeaponScript.OnWeaponSet -= OnWeaponSet;
        WeaponScript.OnBulletFired -= OnCurrentBulletsChanged;
        WeaponScript.OnReloadFinished -= OnReloadFinished;
    }

    private void OnWeaponSet(WeaponStats stats)
    {
        m_weaponImage.sprite = stats.icon;
        m_weaponSprite = stats.icon;

        m_maxAmmoText.text = stats.magazineSize.ToString();
        m_magazineSize = stats.magazineSize;

        m_currentAmmoText.text = stats.magazineSize.ToString();
        m_currentMagazine = stats.magazineSize;
    }

    private void OnCurrentBulletsChanged(int amount)
    {
        m_currentAmmoText.text = amount.ToString();

        if (amount <= (m_magazineSize / 6) && amount != 0)
        {
            m_reloadText.text = "RELOAD";
            m_reloadObject.alpha = 1;
            m_blink = true;
        }
    }

    private void OnReloadFinished(WeaponScript script)
    {
        m_currentMagazine = script.bulletsLeft;
        m_currentAmmoText.text = m_currentMagazine.ToString();
        
        m_reloadText.text = " ";
        m_reloadObject.alpha = 0;
        m_blink = false;
    }
    

    // Update is called once per frame
    private void Update()
    {
        if (m_blink)
        {
            m_reloadText.color = Lerp(m_defaultColour, m_highlightColour, 6);
        }
    }

    private static Color Lerp(Color firstColor, Color secondColor, float speed)
    {
        return Color.Lerp(firstColor, secondColor, Mathf.Sin(Time.time * speed));
    }
}