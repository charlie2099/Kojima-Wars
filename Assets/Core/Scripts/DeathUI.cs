using Core;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class DeathUI : MonoBehaviour
{
    [SerializeField] private GameObject m_gameobjectHolder = default;

    [Header("Respawning")]
    [SerializeField] private Button m_respawnButton = default;
    [SerializeField] private TextMeshProUGUI m_respawnButtonText = default;
    [SerializeField] private Camera m_miniMapCamera = default;
    [SerializeField] private RectTransform m_miniMapImage = default;
    [SerializeField] private RespawnBaseSelector m_respawnBaseSelectorPrefab = default;
    [SerializeField] private Image m_selectedIcon = default;
    [SerializeField] private RectTransform m_selectedIconTransform = default;

    [Header("Values")]
    [SerializeField] private int m_respawnDelayInSeconds = 3;

    private PauseManager m_pauseManager = null;
    private Entity m_entity = null;

    private Dictionary<BaseController, RespawnBaseSelector> m_baseSelectorMap = new Dictionary<BaseController, RespawnBaseSelector>();

    private BaseController m_selectedBase = null;

    private bool m_respawnCountdownComplete = false;

    //private Transform m_followCache = default;

    private void Awake()
    {
        m_gameobjectHolder.SetActive(false);

        m_selectedIcon.enabled = false;

        m_pauseManager = FindObjectOfType<PauseManager>();
    }

    private void Start()
    {
        //AudioManager.Instance.CreateFMODInstance(AudioManager.Instance.events.playerAudioEvents.respawn, 1);
    }

    private void OnEnable()
    {
        StartCoroutine(DelayedEnable());

        m_respawnButton.onClick.AddListener(HandleRespawn);
    }

    IEnumerator DelayedEnable()
    {
        yield return new WaitForSeconds(1f);
        //MechCharacterController.OnLocalMechDead += OnLocalPlayerDead;
        CombatComponent.OnDeath += OnLocalPlayerDead;

        //m_respawnLocationsButton.onClick.AddListener(() => UpdateTabDisplay(true));
        //m_respawnButton.onClick.AddListener(HandleRespawn);
    }

    private void Update()
    {
        if (m_gameobjectHolder.activeSelf)
        {
            foreach (KeyValuePair<BaseController, RespawnBaseSelector> map in m_baseSelectorMap)
            {
                map.Value.UpdateDisplay(m_entity, map.Key);
            }
        }

        if (m_selectedBase != null && (m_selectedBase.State != EBaseState.IDLE || m_selectedBase.TeamOwner != m_entity.TeamName))
        {
            m_selectedBase = null;
            m_selectedIcon.enabled = false;
        }

        m_respawnButton.interactable = m_respawnCountdownComplete && m_selectedBase != null;
    }

    private void OnDisable()
    {
        CombatComponent.OnDeath -= OnLocalPlayerDead;
        CursorManager.DisableCursor("death-ui");
    }

    private void OnLocalPlayerDead(Entity entity)
    {
        //Hide the rest of the games UI elements
        UIManager.Instance.FadeGroup(false, 0.25f, new List<UIGroupName>() { UIGroupName.GAME, UIGroupName.PLAYER });

        //Fade in the death screen element
        UIManager.Instance.FadeGroup(true, 0.25f, new List<UIGroupName>() { UIGroupName.DEATH });

        if (m_pauseManager.paused)
        {
            m_pauseManager.ChangePauseState();
        }

        m_pauseManager.SetCanPause(false);

        if (m_entity == null)
        {
            m_entity = entity;
            SetupBaseLocations();
        }

        m_respawnCountdownComplete = false;
        m_selectedBase = null;
        m_selectedIcon.enabled = false;

        CursorManager.EnableCursor("death-ui");

        m_gameobjectHolder.SetActive(true);
        m_respawnButton.interactable = false;

        //m_followCache = VCameraSetup.playerCam.GetVirtualCamera().Follow;
        //VCameraSetup.playerCam.GetVirtualCamera().Follow = null;
        //VCameraSetup.playerCam.GetVirtualCamera().gameObject.SetActive(true);
        //VCameraSetup.playerCam.GetVirtualCamera().transform.rotation = m_entity.transform.rotation;

        StartCoroutine(UpdateRespawnTimer());
    }

    public IEnumerator UpdateRespawnTimer()
    {
        m_respawnButtonText.text = $"Respawn ({m_respawnDelayInSeconds} seconds)";

        for (int i = 0; i < m_respawnDelayInSeconds; i++)
        {
            yield return new WaitForSeconds(1);
            m_respawnButtonText.text = $"Respawn ({m_respawnDelayInSeconds - i} seconds)";
        }

        m_respawnButtonText.text = "Respawn";
        m_respawnButton.interactable = true;
        m_respawnCountdownComplete = true;
    }

    private void HandleRespawn()
    {
        //Hide the rest of the games UI elements
        //may need to fade out more elements then this 
        UIManager.Instance.FadeGroup(true, 0.25f, new List<UIGroupName>() {UIGroupName.SKILL});
     
        //Fade in the unit info
        UIManager.Instance.FadeGroup(false, 0.25f, new List<UIGroupName>() { UIGroupName.DEATH });

        ulong respawningPlayerId = NetworkManager.Singleton.LocalClientId;

        var nps = NetworkPlayerSetup.Get();

        Transform selectedBaseTransform = m_selectedBase.PlayerStartPosition.transform;
        nps.RespawnPlayerServerRpc(selectedBaseTransform.position, selectedBaseTransform.rotation, respawningPlayerId);

        m_gameobjectHolder.SetActive(false);
        CursorManager.DisableCursor("death-ui");

        m_pauseManager.SetCanPause(true);
        //VCameraSetup.playerCam.GetVirtualCamera().Follow = m_followCache;

        AudioManager.Instance.PlayLocalFMODOneShot(AudioManager.Instance.events.playerAudioEvents.respawn, transform.position);
    }

    private void SetupBaseLocations()
    {
        BaseController[] baseControllers = FindObjectsOfType<BaseController>();
        m_baseSelectorMap = new Dictionary<BaseController, RespawnBaseSelector>();

        foreach (BaseController controller in baseControllers)
        {
            RespawnBaseSelector selector = Instantiate(m_respawnBaseSelectorPrefab, m_miniMapImage);
            selector.SetAnchorPosition(m_miniMapCamera.WorldToScreenPoint(controller.transform.position));
            selector.UpdateDisplay(m_entity, controller);
            selector.SelectButton.onClick.AddListener(() => OnSelectBase(controller));

            m_baseSelectorMap.Add(controller, selector);
        }
    }

    private void OnSelectBase(BaseController controller)
    {
        m_selectedBase = controller;

        m_selectedIcon.enabled = true;
        m_selectedIcon.transform.SetParent(m_baseSelectorMap[controller].transform);
        m_selectedIconTransform.anchoredPosition = new Vector3(0, 0, 50);
    }
}