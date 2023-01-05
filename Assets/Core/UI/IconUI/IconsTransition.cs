using System;
using System.Collections;
using System.Collections.Generic;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class IconsTransition : MonoBehaviour
{
    public GameObject mechIcon;
    public GameObject vtolicon;

    public JetShoot jetShoot;
    public VTOLCharacterController vtol;

    public GameObject holder;

    public TextMeshProUGUI speedText;
    public TextMeshProUGUI altText;

    public Slider slider;

    void Start()
    {
        holder.SetActive(false);
        StartCoroutine(DelayedStart());
    }

    private IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(0.2f);
        
        NetworkTransformComponent.OnMechTransform += NetworkTransformComponentOnOnMechTransform;
        NetworkTransformComponent.OnVTOLTransform += NetworkTransformComponentOnOnVTOLTransform;

        NetworkTransformComponentOnOnMechTransform(null);
    }

    private void NetworkTransformComponentOnOnVTOLTransform(NetworkTransformComponent obj)
    {
        mechIcon.SetActive(false);
        vtolicon.SetActive(true);

        holder.SetActive(true);

        vtol = obj.GetComponent<VTOLCharacterController>();
        jetShoot = obj.GetComponent<JetShoot>();
    }

    private void NetworkTransformComponentOnOnMechTransform(NetworkTransformComponent obj)
    {
        vtolicon.SetActive(false);
        mechIcon.SetActive(true);

        holder.SetActive(false);
    }

    void Update()
    {
        if (holder.activeSelf)
        {
            if (vtol != null)
            {
                if (vtol.rb.velocity.magnitude > 95)
                {
                    speedText.text = "SPD: " + 100;
                }
                else
                {
                    speedText.text = "SPD: " + Mathf.RoundToInt(vtol.rb.velocity.magnitude).ToString();
                }

                altText.text = "ALT: " + Mathf.RoundToInt(vtol.altitudRef).ToString();
                slider.value = jetShoot.overheatTime;
            }
        }
    }
}