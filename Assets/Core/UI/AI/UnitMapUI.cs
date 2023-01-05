using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UnitMapUI : MonoBehaviour
{  
    [Header("Map")]
    [SerializeField] private GameObject m_gameobjectHolder = default;
    [SerializeField] private Camera m_miniMapCamera = default;
    [SerializeField] private RectTransform m_miniMapImage = default;
    [SerializeField] private UnitBaseSelector m_baseSelectorPrefab = default;
    [SerializeField] private Image m_selectedIcon = default;
    [SerializeField] private RectTransform m_selectedIconTransform = default;

    [Header("Unit Info")]
    [SerializeField] private Transform m_unitInfoHolder = default;
    [SerializeField] private TextMeshProUGUI m_unitGroupText = default;
    [SerializeField] private UnitInfoUI m_infantryInfoUI = default;
    [SerializeField] private UnitInfoUI m_tankInfoUI = default;
    [SerializeField] private UnitInfoUI m_helicopterInfoUI = default;
    [SerializeField] private PlayerDataSO playerDataSO = default;

    [SerializeField] private GameController controller;
    public AIGroup aiGroup;

    [Header("Unit Buttons")]
    [SerializeField] private List<Button> m_unitGroupButtons = default;

    private PauseManager m_pauseManager = null;
    private Entity m_playerController = null;

    private Dictionary<BaseController, UnitBaseSelector> m_baseSelectorMap = new Dictionary<BaseController, UnitBaseSelector>();

    private int m_selectedUnitGroup = -1;
    private bool m_isMapOpen = false;

    public void OnSelectedGroupEmpty(int groupIndex, ulong localPlayerId)
    {
        m_selectedUnitGroup = groupIndex;

        SetUnitInfoUI(groupIndex);
        m_unitInfoHolder.gameObject.SetActive(true);
        GameController gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        gameController.SelectAIGroupServerRpc(localPlayerId, groupIndex);

        foreach (KeyValuePair<BaseController, UnitBaseSelector> selector in m_baseSelectorMap)
        {
            gameController.MakeSelectorButtonForBaseInteractableServerRpc(localPlayerId, selector.Key.GetBaseId(), groupIndex);
        }
    }

    public void OnSelectedGroupNotEmpty()
    {
        foreach (KeyValuePair<BaseController, UnitBaseSelector> selector in m_baseSelectorMap)
        {
            selector.Value.SelectButton.interactable = false;
        }

        m_selectedIcon.enabled = false;
    }

    public void UpdateInfantryUI(bool active, int quantity)
    {
        m_infantryInfoUI.gameObject.SetActive(active);
        m_infantryInfoUI.UpdateQuantity(quantity);
    }

    public void UpdateTankUI(bool active, int quantity)
    {
        m_tankInfoUI.gameObject.SetActive(active);
        m_tankInfoUI.UpdateQuantity(quantity);
    }

    public void UpdateHelicopterUI(bool active, int quantity)
    {
        m_helicopterInfoUI.gameObject.SetActive(active);
        m_helicopterInfoUI.UpdateQuantity(quantity);
    }

    public void MakeSelectorButtonForBaseInteractable(int baseId, bool interactable)
    {
        foreach (KeyValuePair<BaseController, UnitBaseSelector> selector in m_baseSelectorMap)
        {
            if(selector.Key.GetBaseId() == baseId)
            {
                selector.Value.SelectButton.interactable = interactable;
            }
        }
    }

    public void MakeUnitGroupButtonInteractable(int buttonIndex, bool interactable)
    {
        m_unitGroupButtons[buttonIndex].interactable = interactable;
    }

    public void DeselectButton(int buttonIndex)
    {
        if (m_selectedUnitGroup == buttonIndex)
        {
            m_selectedUnitGroup = -1;
            m_selectedIcon.enabled = false;
        }
    }

    private void Awake()
    {
        m_gameobjectHolder.SetActive(false);
        m_selectedIcon.enabled = false;

        m_pauseManager = FindObjectOfType<PauseManager>();
    }

    private void OnEnable()
    {
        InputManager.MECH.UnitMap.performed += OnToggleMap;
    }

    private void OnDisable()
    {
        InputManager.MECH.UnitMap.performed -= OnToggleMap;
    }

    private void OnToggleMap(InputAction.CallbackContext context)
    {
        // toggle the map
        if (m_isMapOpen)
        {
            CloseMap();
        }
        else
        {
            OpenMap();
        }

        m_isMapOpen = !m_isMapOpen;
    }

    private void OpenMap()
    {
        //Hide the rest of the games UI elements
        /*
        UIManager.Instance.FadeGroup(false, 0.25f, new List<UIGroupName>() { UIGroupName.GAME, UIGroupName.PLAYER });

        //Fade in the unit info
        UIManager.Instance.FadeGroup(true, 0.25f, new List<UIGroupName>() { UIGroupName.UNIT });
        */

        InputManager.SetInputType(ControlType.NONE);
        m_pauseManager.SetCanPause(false);

        foreach (Button button in m_unitGroupButtons)
        {
            button.onClick.AddListener(() => OnUnitGroupSelected(m_unitGroupButtons.IndexOf(button), NetworkManager.Singleton.LocalClientId));
        }

        InputManager.MECH.UnitMap.Enable();
        
        if (m_playerController == null)
        {
            foreach (MechCharacterController mech in MechCharacterController.List)
            {
                if (mech.OwnerClientId == mech.NetworkManager.LocalClientId)
                {
                    m_playerController = mech.GetComponent<Entity>();
                    SetupBaseLocations();
                    break;
                }
            }
        }

        foreach (KeyValuePair<BaseController, UnitBaseSelector> selector in m_baseSelectorMap)
        {
            selector.Value.SelectButton.interactable = false;
        }

        GameController gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        int count = 0;
        foreach(Button button in m_unitGroupButtons)
        {
            // Find the number of agents in each group. Button is interactable if it is greater than 0
            // playerData only exists on the server
            gameController.MakeUnitGroupButtonInteractableServerRpc(NetworkManager.Singleton.LocalClientId, count);

            count++;
        }

        m_selectedUnitGroup = -1;
        m_selectedIcon.enabled = false;

        CursorManager.EnableCursor("ai-ui");

        m_gameobjectHolder.SetActive(true);
        m_unitInfoHolder.gameObject.SetActive(false);
    }

    private void CloseMap()
    {
        /*
        //Hide the rest of the games UI elements
        UIManager.Instance.FadeGroup(true, 0.25f, new List<UIGroupName>() { UIGroupName.GAME, UIGroupName.PLAYER });

        //Fade in the unit info
        UIManager.Instance.FadeGroup(false, 0.25f, new List<UIGroupName>() { UIGroupName.UNIT });
        */

        InputManager.SetInputType(ControlType.MECH);
        m_pauseManager.SetCanPause(true);

        foreach (Button button in m_unitGroupButtons)
        {
            button.onClick.RemoveAllListeners();
        }

        CursorManager.DisableCursor("ai-ui");

        m_gameobjectHolder.SetActive(false);

        foreach (Button button in m_unitGroupButtons)
        {
            button.onClick.RemoveAllListeners();
        }
    }

    private void FixedUpdate() // Fixed just to reduce the number of times this is called as messages with data are sent over the network each call
    {
        if (m_gameobjectHolder.activeSelf)
        {
            foreach (KeyValuePair<BaseController, UnitBaseSelector> map in m_baseSelectorMap)
            {
                map.Value.UpdateDisplay(m_playerController, map.Key);
            }

            GameController gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
            for (int i = 0; i < m_unitGroupButtons.Count; i++)
            {
                gameController.CheckAIGroupButtonValidServerRpc(NetworkManager.Singleton.LocalClientId, i);
            }
        }
    }

    private void SetupBaseLocations()
    {
        BaseController[] baseControllers = FindObjectsOfType<BaseController>();
        m_baseSelectorMap = new Dictionary<BaseController, UnitBaseSelector>();

        foreach (BaseController controller in baseControllers)
        {
            UnitBaseSelector selector = Instantiate(m_baseSelectorPrefab, m_miniMapImage);
            selector.SelectButton.onClick.RemoveAllListeners();
            selector.SetAnchorPosition(m_miniMapCamera.WorldToScreenPoint(controller.transform.position));
            selector.UpdateDisplay(m_playerController.GetComponent<Entity>(), controller);
            selector.SelectButton.onClick.AddListener(() => OnSelectBase(controller, NetworkManager.Singleton.LocalClientId));

            m_baseSelectorMap.Add(controller, selector);
        }
    }

    private void OnSelectBase(BaseController controller, ulong localPlayerId)
    {
        int baseId = controller.GetBaseId();
        
        GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().SelectBaseAIMapServerRpc(localPlayerId, baseId, m_selectedUnitGroup);

        //GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().SelectBaseAIMapServerRpc(localPlayerId, baseId, m_selectedUnitGroup);
        OnUnitGroupSelected(m_selectedUnitGroup, localPlayerId);
    }

    private void OnUnitGroupSelected(int index, ulong localPlayerId)
    {
        GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().OnSelectAIGroupServerRpc(localPlayerId, index);
    }

    public void SetUnitInfoUI(int index)
    {
        GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().UpdateUnitInfoUIForGroupServerRpc(
            NetworkManager.Singleton.LocalClientId, index);
    }

    public void SetBaseSelectionIcons(ulong localPlayerId, int baseId)
    {
        BaseController selectedBaseController = null;
        BaseController[] baseControllers = GameObject.FindObjectsOfType<BaseController>();
        foreach(BaseController baseController in baseControllers)
        {
            if(baseId == baseController.GetBaseId())
            {
                selectedBaseController = baseController;
                break;
            }
        }

        m_baseSelectorMap[selectedBaseController].SelectButton.interactable = false;
        
        m_selectedIcon.transform.SetParent(m_baseSelectorMap[selectedBaseController].transform);
        m_selectedIconTransform.anchoredPosition = new Vector3(0, 0, 50);
        m_selectedIcon.enabled = true;
    }
}