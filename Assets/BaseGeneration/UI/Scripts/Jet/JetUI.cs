using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Networking;
using UnityEngine.UI;

public class JetUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject m_holder;
    [SerializeField] private TextMeshProUGUI m_speedText;
    [SerializeField] private TextMeshProUGUI m_altitudeText;
    [SerializeField] private Slider m_slider;
    
    private JetShoot m_jetShootReference;
    private VTOLCharacterController m_vtolControllerReference;
    
    void Start()
    {
        m_holder.SetActive(false);
        StartCoroutine(DelayedStart());
    }

    private IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(0.2f);
        
        NetworkTransformComponent.OnMechTransform += OnMechTransform;
        NetworkTransformComponent.OnVTOLTransform += OnVTOLTransform;

        OnMechTransform(null);
    }

    private void OnDestroy()
    {
        NetworkTransformComponent.OnMechTransform -= OnMechTransform;
        NetworkTransformComponent.OnVTOLTransform -= OnVTOLTransform;
    }

    private void OnVTOLTransform(NetworkTransformComponent obj)
    {
        m_holder.SetActive(true);
        m_vtolControllerReference = obj.GetComponent<VTOLCharacterController>();
        m_jetShootReference = obj.GetComponent<JetShoot>();
    }

    private void OnMechTransform(NetworkTransformComponent obj)
    {
        m_holder.SetActive(false);
    }
    
    void Update()
    {
        if (m_holder.activeSelf)
        {
            if (m_vtolControllerReference != null)
            {
                if (m_vtolControllerReference.rb.velocity.magnitude > 95)
                {
                    m_speedText.text = "SPD: " + 100;
                }
                else
                {
                    m_speedText.text = "SPD: " + Mathf.RoundToInt(m_vtolControllerReference.rb.velocity.magnitude).ToString();
                }

                m_altitudeText.text = "ALT: " + Mathf.RoundToInt(m_vtolControllerReference.altitudRef).ToString();
                m_slider.value = m_jetShootReference.overheatTime;
            }
        }
    }
}
