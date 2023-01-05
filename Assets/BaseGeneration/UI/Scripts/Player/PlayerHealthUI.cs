using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Unity.Netcode;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Health Settings")] 
    [SerializeField] private Color healthColour;
    [SerializeField] private Color shieldColour;
    [SerializeField] private Image healthFill;
    [SerializeField] private Image shieldFill;

    [Header("Hit Feedback Settings")] 
    [SerializeField] private Color hitFeedbackColour;
    [SerializeField] private Color healingColour;
    [SerializeField] private float m_feedbackIntensity = 0.25f;
    [SerializeField] private float m_feedbackTime = 0.05f;
    
    private int m_maxHealth = 100;
    private int m_maxShields = 200;


    ulong ownerID;

    private Volume m_postProcessingVolume;
    private Color m_hitFeedbackColour;

    private void Start()
    {
        healthFill.color = healthColour;
        shieldFill.color = shieldColour;

        StartCoroutine(DelayStart());
    }


    IEnumerator DelayStart()
    {
        yield return new WaitForSeconds(0.2f);
        m_postProcessingVolume = GameObject.Find("Post Processing Volume").GetComponent<Volume>();

        // register callbacks for setting the ui
        ownerID = NetworkManager.Singleton.LocalClientId;

        HealthComponent.GetCallback(ownerID).AddListener(OnPlayerHealthUpdated);
        ShieldComponent.GetCallback(ownerID).AddListener(OnPlayerShieldsUpdated);
    }

    public void OnPlayerShieldsUpdated(int oldVal, int newVal)
    {
        // set the UI
        float shield = (float)newVal / m_maxShields;
        shieldFill.fillAmount = shield;

        if(shield >= 0 && oldVal > newVal)
        {
            m_hitFeedbackColour = hitFeedbackColour;
            StartCoroutine(FadeInHitFeedback(0.0f, m_feedbackIntensity, m_feedbackTime));
        }
        if(shield >= 0 && oldVal < newVal)
        {
            m_hitFeedbackColour = healingColour;
            StartCoroutine(FadeInHitFeedback(0.0f, m_feedbackIntensity, m_feedbackTime));
        }
    }

    public void OnPlayerHealthUpdated(int oldVal, int newVal)
    {
        float health = (float)newVal / m_maxHealth;
        healthFill.fillAmount = health;


        if (health >= 0 && oldVal > newVal)
        {
            m_hitFeedbackColour = hitFeedbackColour;
            CheckCurrentHealth(health);
        }

        if (health >= 0 && oldVal < newVal)
        {
            m_hitFeedbackColour = healingColour;
            CheckCurrentHealth(health);
        }
        
    }

    private IEnumerator FadeInHitFeedback(float start, float end, float lerpTime)
    {
        float timeStartedLerping = Time.time;
        float timeSinceStart = Time.time - timeStartedLerping;
        float percentageComplete = timeSinceStart / lerpTime;
        
        while (true)
        {
            timeSinceStart = Time.time - timeStartedLerping;
            percentageComplete = timeSinceStart / lerpTime;
            float currentValue = Mathf.Lerp(start, end, percentageComplete);

            if (m_postProcessingVolume.profile.TryGet<Vignette>(out var vignette))
            {
                vignette.color.value = m_hitFeedbackColour;
                vignette.intensity.value = currentValue;
            }

            if (m_postProcessingVolume.profile.TryGet<ChromaticAberration>(out var aberration))
            {
                aberration.intensity.value = currentValue * 2;
            }

            if (percentageComplete >= 1) break;

            yield return new WaitForEndOfFrame();
        }

        StartCoroutine(FadeOutHitFeedback(end, start, m_feedbackTime));
    }

    private IEnumerator FadeOutHitFeedback(float start, float end, float lerpTime)
    {
        float timeStartedLerping = Time.time;
        float timeSinceStart = Time.time - timeStartedLerping;
        float percentageComplete = timeSinceStart / lerpTime;

        while (true)
        {
            timeSinceStart = Time.time - timeStartedLerping;
            percentageComplete = timeSinceStart / lerpTime;
            float currentValue = Mathf.Lerp(start, end, percentageComplete);

            if (m_postProcessingVolume.profile.TryGet<Vignette>(out var vignette))
            {
                vignette.color.value = m_hitFeedbackColour;
                vignette.intensity.value = currentValue;
            }
            
            if (m_postProcessingVolume.profile.TryGet<ChromaticAberration>(out var aberration))
            {
                aberration.intensity.value = currentValue;
            }

            if (percentageComplete >= 1) break;

            yield return new WaitForEndOfFrame();
        }
    }

    private void CheckCurrentHealth(float health)
    {
        float newHealth = health - 1f;
        newHealth *= 2.5f;
        //print(newHealth);
        float vignetteValue = 0.004f * -newHealth;
        if (m_postProcessingVolume.profile.TryGet<Vignette>(out var vignette))
        {
            vignette.intensity.value = vignetteValue;
        }
        StartCoroutine(FadeInHitFeedback(vignetteValue, vignetteValue + m_feedbackIntensity, m_feedbackTime));
    }
}