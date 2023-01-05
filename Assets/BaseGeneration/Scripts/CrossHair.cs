using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CrossHair : NetworkBehaviour
{
    [Header("Crosshair Panels")]
    [SerializeField] private List<RectTransform> m_crosshairRects = new List<RectTransform>();
    [SerializeField] private List<GameObject> m_crosshairObjects = new List<GameObject>();
    [SerializeField] private GameObject m_dotPanel;
    
    [Header("Crosshair Settings")]
    public float idleSize = 50f;
    public float shootingSize = 100f;
    public float currentSize = 50f;
    public float speed = 10f;
    
    [Header("Hitmarker Panels")]
    [SerializeField] private RectTransform m_hitmarkerRect;

    [Header("Hitmarker Settings")] 
    [SerializeField] private Color m_hitMarkerColour;
    [SerializeField] private float m_hitMarkerInitialSize = 30;
    [SerializeField] private float m_hitMarkerMaxSize = 50f;
    [SerializeField] private float m_hitMarkerSpeed = 10.0f;
    [SerializeField] private List<Image> m_hitMarkerElements = new List<Image>();
    
    private int m_currentWeapon;
    private float m_hitMarkerView;
    private float m_hitMarkerCurrentSize;
    private bool m_hitMarkerVisible;

    private float m_spread;
    private bool m_firing;
    private float m_hitMarkerSizeIncrease = 0.32f;
    
    // do we need to store this in the crosshair?
    public ulong personalID;
    public ulong clientID;
    
    // Start is called before the first frame update
    private void Start()
    {
        StartCoroutine(DelayedStart());
        m_hitMarkerCurrentSize = m_hitMarkerInitialSize;
        m_hitMarkerView = 0;

        if(AudioManager.Instance != null)
        {
            AudioManager.Instance.CreateFMODInstance(AudioManager.Instance.events.uIAudioEvents.hitmarker, 10);
        }
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(0.2f);
        //WeaponScript.OnCurrentWeaponUsed += OnCurrentWeapon;
        ServerAbilityManager.OnHitEntityDetected += OnHitDetected;
        WeaponScript.OnWeaponSet += OnWeaponSet;
        WeaponScript.OnWeaponFired += OnWeaponFired;
        
        SetHitMarkerAlpha(0);
    }
    
    private void OnDisable()
    {
        //WeaponScript.OnCurrentWeaponUsed -= OnCurrentWeapon;
        ServerAbilityManager.OnHitEntityDetected -= OnHitDetected;
        WeaponScript.OnWeaponSet -= OnWeaponSet;
        WeaponScript.OnWeaponFired -= OnWeaponFired;
    }
    
    private void OnWeaponSet(WeaponStats obj)
    {
        m_spread = obj.hipfireSpread;
    }

    // Update is called once per frame
    private void Update()
    {
        if (m_firing)
        {
            currentSize = Mathf.Lerp(currentSize, shootingSize * m_spread / 1.35f, Time.deltaTime * speed);
        }
        else
        {
            currentSize = Mathf.Lerp(currentSize, idleSize, Time.deltaTime * speed);
        }

        m_crosshairRects[m_currentWeapon].sizeDelta = new Vector2(currentSize, currentSize);

        if (InputManager.MECH.ADS.IsPressed())
        {
            m_crosshairObjects[m_currentWeapon].SetActive(false);
        }
        else
        {
            m_crosshairObjects[m_currentWeapon].SetActive(true);
        }
        
        if (m_hitMarkerView > 0)
        {
            SetHitMarkerAlpha(Mathf.Lerp(0,255, Time.deltaTime * m_hitMarkerSpeed));
            m_hitMarkerCurrentSize = Mathf.Lerp(m_hitMarkerCurrentSize, m_hitMarkerMaxSize, Time.deltaTime * m_hitMarkerSpeed);
            m_hitmarkerRect.sizeDelta = new Vector2(m_hitMarkerCurrentSize, m_hitMarkerCurrentSize);
            m_hitMarkerView -= Time.deltaTime; 
            
            foreach (var images in m_hitMarkerElements)
            {
                var rectTransform = images.rectTransform;
                var sizeDelta = rectTransform.sizeDelta;
                var amount = sizeDelta.x;
                
                sizeDelta = new Vector2(amount += m_hitMarkerSizeIncrease, sizeDelta.y);
                
                rectTransform.sizeDelta = sizeDelta;
            }
        }
        else
        {
            m_hitMarkerView = 0;
            SetHitMarkerAlpha(Mathf.Lerp(m_hitMarkerElements[0].color.a,0, Time.deltaTime * m_hitMarkerSpeed * 2));                                                 
            m_hitMarkerCurrentSize = Mathf.Lerp(m_hitMarkerCurrentSize, m_hitMarkerInitialSize, Time.deltaTime * m_hitMarkerSpeed);            
            m_hitmarkerRect.sizeDelta = new Vector2(m_hitMarkerCurrentSize, m_hitMarkerCurrentSize);

            foreach (var images in m_hitMarkerElements)
            {
                var rectTransform = images.rectTransform;
                var sizeDelta = rectTransform.sizeDelta;

                sizeDelta = new Vector2(30.0f, sizeDelta.y);
                rectTransform.sizeDelta = sizeDelta;
            }
        }
    }

    private void OnWeaponFired(bool firing)
    {
        m_firing = firing;
    }
    
    // show the hitmarker
    private void OnHitDetected(ulong clientShot)
    {
        clientID = clientShot;
        
        // detect if the client that shot was this client.
        if (clientID == NetworkManager.LocalClient.ClientId)
        {
            // can call the hit marker sound in here somewhere
            m_hitMarkerView = 0.3f;
        }
    }
    
    private void SetHitMarkerAlpha(float a)
    {
        var tempColour = m_hitMarkerColour;
        tempColour.a = a;

        foreach (var image in m_hitMarkerElements)
        {
            image.color = tempColour;
        }
    }


    private void OnCurrentWeapon(string weapon)
    {
        // can we add these to the weapons scriptable objets and pass that as a reference? then we can spawn the correct
        // crosshair panel from there and assign all of things? if the weapon names are ever changed this will always fail.
        
        if(weapon == "Assault Rifle")
        {
            m_currentWeapon = 0;
            m_crosshairObjects[0].SetActive(true);
        }
        else
        {
            m_crosshairObjects[0].SetActive(false);
        }
        if (weapon == "BurstAR")
        {
            m_currentWeapon = 1;

            m_crosshairObjects[1].SetActive(true);
        }
        else
        {
            m_crosshairObjects[1].SetActive(false);
        }
        if (weapon == "Shotgun")
        {
            m_currentWeapon = 2;

            m_crosshairObjects[2].SetActive(true);
        }
        else
        {
            m_crosshairObjects[2].SetActive(false);
        }
        if (weapon == "SniperRifle")
        {
            m_currentWeapon = 3;

            m_crosshairObjects[3].SetActive(true);
        }
        else
        {
            m_crosshairObjects[3].SetActive(false);
        }
        if (weapon == "Pistol")
        {
            m_currentWeapon = 4;

            m_crosshairObjects[4].SetActive(true);
        }
        else
        {
            m_crosshairObjects[4].SetActive(false);
        }

    }
}