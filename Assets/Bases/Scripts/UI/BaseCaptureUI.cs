using System.Collections.Generic;
using FMOD.Studio;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class BaseCaptureUI : MonoBehaviour
{
    //[Tooltip("The gameobject that holds all the capture UI, to be enabled/disabled on entering/leaving the zone.")]
    //[SerializeField] private GameObject m_uiHolder = default;
    //[SerializeField] private GameTeamData m_gameTeamData = default;
    
    //[SerializeField] private Image m_fillImage;
    //[SerializeField] private TextMeshProUGUI m_captureStatusText;

    //private List<BaseCaptureZone> m_baseCaptureZones = new List<BaseCaptureZone>();
    //private BaseCaptureZone currentZone = null;

    //private FMOD.Studio.EventInstance baseCapturingAudio;

    //private void Awake()
    //{
    //    m_uiHolder.SetActive(false);
    //}

    //private void Start()
    //{
    //    baseCapturingAudio = FMODUnity.RuntimeManager.CreateInstance(AudioManager.Instance.events.uIAudioEvents.baseCapturing);
    //}

    //private void OnEnable()
    //{
    //    foreach (BaseCaptureZone zone in FindObjectsOfType<BaseCaptureZone>())
    //    {
    //        //zone.OnEntityEntered += OnEntityEntered;
    //        //zone.OnEntityExited += OnEntityExited;
    //        //zone.OnCaptureProgressUpdated += OnCaptureProgressUpdated;
    //        //zone.OnCaptureStatusUpdated += OnCaptureStatusUpdated;
    //        //zone.OnBaseCaptured += OnBaseCaptured;
    //        //zone.BaseController.OnStateChanged += (state) => OnStateChanged(zone, state);
    //        //m_baseCaptureZones.Add(zone);
    //    }
    //}

    //private void OnDisable()
    //{
    //    foreach (BaseCaptureZone zone in m_baseCaptureZones)
    //    {
    //        zone.OnEntityEntered -= OnEntityEntered;
    //        zone.OnEntityExited -= OnEntityExited;
    //        zone.OnCaptureProgressUpdated -= OnCaptureProgressUpdated;
    //        zone.OnCaptureStatusUpdated -= OnCaptureStatusUpdated;
    //        zone.OnBaseCaptured -= OnBaseCaptured;
    //        zone.BaseController.OnStateChanged -= (state) => OnStateChanged(zone, state);
    //    }
    //}

    //private void OnStateChanged(BaseCaptureZone zone, EBaseState state)
    //{
    //    if (state != EBaseState.IDLE && zone.IsLocalPlayerInZone())
    //    {
    //        m_uiHolder.SetActive(true);
    //    }
    //}

    //private void OnEntityEntered(Entity entity, BaseCaptureZone zone, int baseIdEntered)
    //{
    //    if (zone.ContestingTeam == "") return;

    //    NetworkObject networkedObject = entity.GetComponent<NetworkObject>();
    //    if (entity.EntityType == Entity.EEntityType.PLAYER && networkedObject != null && networkedObject.OwnerClientId == networkedObject.NetworkManager.LocalClientId)
    //    {
    //        m_uiHolder.SetActive(true);
    //        m_fillImage.color = m_gameTeamData.GetTeamData(zone.ContestingTeam).Colour;
    //        m_fillImage.fillAmount = zone.Progress;

    //        if (AudioManager.Instance.GetPlaybackState(baseCapturingAudio) != PLAYBACK_STATE.PLAYING)
    //        {
    //            baseCapturingAudio.start();
    //        }
    //    }
    //}

    //private void OnEntityExited(Entity entity, int baseIdExited)
    //{
    //    NetworkObject networkedObject = entity.GetComponent<NetworkObject>();
    //    if (entity.EntityType == Entity.EEntityType.PLAYER && networkedObject != null && networkedObject.OwnerClientId == networkedObject.NetworkManager.LocalClientId)
    //    {
    //        m_uiHolder.SetActive(false);

    //        if (AudioManager.Instance.GetPlaybackState(baseCapturingAudio) == PLAYBACK_STATE.PLAYING)
    //        {
    //            baseCapturingAudio.stop(STOP_MODE.ALLOWFADEOUT);
    //        }
    //    }
    //}

    //private void OnCaptureProgressUpdated(float progress)
    //{
    //    if (m_uiHolder.activeSelf)
    //    {
    //        m_fillImage.fillAmount = progress;
    //    }
    //}

    //private void OnBaseCaptured(string teamName)
    //{
    //    m_uiHolder.SetActive(false);
    //    baseCapturingAudio.stop(STOP_MODE.ALLOWFADEOUT);
    //}

    //private void OnCaptureStatusUpdated(string status, Color teamColour)
    //{
    //    m_captureStatusText.text = status;
    //}
}