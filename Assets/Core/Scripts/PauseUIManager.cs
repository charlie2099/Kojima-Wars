using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseUIManager : MonoBehaviour
{
    [SerializeField] GameObject pauseUIContainer;
    [SerializeField] GameObject settingsUI;
    [SerializeField] GameObject pauseBackground;

    [SerializeField] Button playButton, settingsButton, quitButton;

    public bool UIActive { get; set; }

    private void Awake()
    {
        pauseUIContainer.SetActive(false);
        settingsUI.SetActive(false);
        pauseBackground.SetActive(false);
    }

    public void OnSettingsPressed()
    {
        settingsUI.SetActive(true);
        pauseBackground.SetActive(false);
        playButton.interactable = false;
        settingsButton.interactable = false;
        quitButton.interactable = false;
    }

    public void OnBackPressed()
    {
        settingsUI.SetActive(false);
        pauseBackground.SetActive(true);
        playButton.interactable = true;
        settingsButton.interactable = true;
        quitButton.interactable = true;
    }

    public void ShowPauseUI()
    {
        //Hide the rest of the games UI elements
        UIManager.Instance.FadeGroup(false, 0.25f, new List<UIGroupName>() { UIGroupName.GAME, UIGroupName.PLAYER });

        //Fade in the pause menu
        UIManager.Instance.FadeGroup(true, 0.25f, new List<UIGroupName>() { UIGroupName.PAUSE });

        pauseUIContainer.SetActive(true);
        pauseBackground.SetActive(true);

        playButton.interactable = true;
        settingsButton.interactable = true;
        quitButton.interactable = true;

        UIActive = true;
    }

    public void HidePauseUI()
    {

        //Hide the rest of the games UI elements
        UIManager.Instance.FadeGroup(true, 0.25f, new List<UIGroupName>() { UIGroupName.GAME, UIGroupName.PLAYER });

        //Fade in the pause menu
        UIManager.Instance.FadeGroup(false, 0.25f, new List<UIGroupName>() { UIGroupName.PAUSE });

        pauseBackground.SetActive(false);
        settingsUI.SetActive(false);
        pauseUIContainer.SetActive(false);

        UIActive = false;
    }
}
