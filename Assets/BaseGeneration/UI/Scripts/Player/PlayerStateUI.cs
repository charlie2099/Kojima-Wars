using System.Collections;
using System.Collections.Generic;
using TMPro;
using Networking;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStateUI : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private TextMeshProUGUI m_classText;
    [SerializeField] private TextMeshProUGUI m_stateText;
    [SerializeField] private Image m_backgroundImage;
    
    void Start()
    {
        StartCoroutine(DelayedStart());
    }

    private IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(0.2f);
        
        NetworkTransformComponent.OnMechTransform += OnMechTransform;
        NetworkTransformComponent.OnVTOLTransform += OnVTOLTransform;
        PlayerClassUI.OnClassAssigned += OnClassAssigned;

        OnMechTransform(null);
    }
    
    private void OnClassAssigned(int index)
    {
        var so = PlayerClassSO.GetClassFromID(index);
        m_classText.text = so.GetClassName();
        m_backgroundImage.color = so.GetClassColour();
    }
    
    private void OnVTOLTransform(NetworkTransformComponent obj)
    {
        m_stateText.text = "VTOL";
    }

    private void OnMechTransform(NetworkTransformComponent obj)
    {
        m_stateText.text = "MECH";
    }

    private void OnDestroy()
    {
        PlayerClassUI.OnClassAssigned -= OnClassAssigned;
    }
}
