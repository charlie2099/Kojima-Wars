using System.Collections.Generic;
using FMOD.Studio;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Core;

public class BaseCaptureUI1 : SingletonComponent<BaseCaptureUI1>
{
    [Header("Team Data Scriptable Object")]
    [SerializeField] private GameTeamData m_gameTeamData = default;

    [Header("UI Elements")]
    [SerializeField] private GameObject m_uiHolder = default;
    [SerializeField] private Image m_fillImage;

    //private FMOD.Studio.EventInstance baseCapturingAudio;

    private void Start()
    {
        // disable ui 
        m_uiHolder.SetActive(false);
        // init FMOD instance
/*        var audioInstance = AudioManager.Instance.events.uIAudioEvents.baseCapturing;
        baseCapturingAudio = FMODUnity.RuntimeManager.CreateInstance(audioInstance);*/
    }

    public void OnEnableUI(ulong localPlayerID, bool enable)
    {
        // only do if is client
        if (!IsLocalClient(localPlayerID)) return;

        // sets the ui visibility 
        m_uiHolder.SetActive(enable);
    }

    public void UpdateUI(ulong localPlayerID, int teamID, float captureProgress)
    {
        // only do if is client
        if (!IsLocalClient(localPlayerID)) return;

        // if the base is contested
        if(teamID == -1)
        {
            m_fillImage.color = Color.gray;
            m_fillImage.fillAmount = captureProgress;
            return;
        }

        // team capturing 
        m_fillImage.color = GetTeamColour(teamID);
        m_fillImage.fillAmount = captureProgress;
    }

    private bool IsLocalClient(ulong localPlayerID)
    {
        // only do if is client
        var thisPlayerID = NetworkManager.Singleton.LocalClientId;
        return thisPlayerID == localPlayerID;
    }

    private Color GetTeamColour(int team)
    {
        return m_gameTeamData.GetTeamDataAtIndex(team).Colour;
    }

    //-----------------------------------------------------------------
    //-----------------------------------------------------------------
    //-----------------------------------------------------------------
    //-----------------------------------------------------------------






/*    private void OnEnable()
    {
        foreach (BaseCaptureZone zone in FindObjectsOfType<BaseCaptureZone>())
        {
            zone.OnEntityEntered += OnEntityEntered;
            zone.OnEntityExited += OnEntityExited;
            zone.OnCaptureProgressUpdated += OnCaptureProgressUpdated;
            zone.OnCaptureStatusUpdated += OnCaptureStatusUpdated;
            zone.OnBaseCaptured += OnBaseCaptured;
            zone.BaseController.OnStateChanged += (state) => OnStateChanged(zone, state);
            m_baseCaptureZones.Add(zone);
        }
    }*/

/*    private void OnDisable()
    {
        foreach (BaseCaptureZone zone in m_baseCaptureZones)
        {
            zone.OnEntityEntered -= OnEntityEntered;
            zone.OnEntityExited -= OnEntityExited;
            zone.OnCaptureProgressUpdated -= OnCaptureProgressUpdated;
            zone.OnCaptureStatusUpdated -= OnCaptureStatusUpdated;
            zone.OnBaseCaptured -= OnBaseCaptured;
            zone.BaseController.OnStateChanged -= (state) => OnStateChanged(zone, state);
        }
    }*/

/*    private void OnStateChanged(BaseCaptureZone zone, EBaseState state)
    {
        if (state != EBaseState.IDLE && zone.IsLocalPlayerInZone())
        {
            m_uiHolder.SetActive(true);
        }
    }*/

/*    private void OnEntityEntered(Entity entity, BaseCaptureZone zone, int baseIdEntered)
    {
        if (zone.ContestingTeam == "") return;

        NetworkObject networkedObject = entity.GetComponent<NetworkObject>();
        if (entity.EntityType == Entity.EEntityType.PLAYER && networkedObject != null && networkedObject.OwnerClientId == networkedObject.NetworkManager.LocalClientId)
        {
            m_uiHolder.SetActive(true);
            m_fillImage.color = m_gameTeamData.GetTeamData(zone.ContestingTeam).Colour;
            m_fillImage.fillAmount = zone.Progress;

            if (AudioManager.Instance.GetPlaybackState(baseCapturingAudio) != PLAYBACK_STATE.PLAYING)
            {
                baseCapturingAudio.start();
            }
        }
    }

    private void OnEntityExited(Entity entity, int baseIdExited)
    {
        NetworkObject networkedObject = entity.GetComponent<NetworkObject>();
        if (entity.EntityType == Entity.EEntityType.PLAYER && networkedObject != null && networkedObject.OwnerClientId == networkedObject.NetworkManager.LocalClientId)
        {
            m_uiHolder.SetActive(false);

            if (AudioManager.Instance.GetPlaybackState(baseCapturingAudio) == PLAYBACK_STATE.PLAYING)
            {
                baseCapturingAudio.stop(STOP_MODE.ALLOWFADEOUT);
            }
        }
    }

    private void OnCaptureProgressUpdated(float progress)
    {
        if (m_uiHolder.activeSelf)
        {
            m_fillImage.fillAmount = progress;
        }
    }

    private void OnBaseCaptured(string teamName)
    {
        m_uiHolder.SetActive(false);
        baseCapturingAudio.stop(STOP_MODE.ALLOWFADEOUT);
    }*/

/*    private void OnCaptureStatusUpdated(string status, Color teamColour)
    {
        m_captureStatusText.text = status;
    }*/
}