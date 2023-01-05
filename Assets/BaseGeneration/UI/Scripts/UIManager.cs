using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Canvas Group References")]
    [SerializeField] private CanvasGroup m_playerHUD;
    [SerializeField] private CanvasGroup m_skillHUD;
    [SerializeField] private CanvasGroup m_gameHUD;
    [SerializeField] private CanvasGroup m_gameFinishedHUD;
    [SerializeField] private CanvasGroup m_deathHUD;
    [SerializeField] private CanvasGroup m_pauseHUD;
    [SerializeField] private CanvasGroup m_unitHUD;

    // need all of the UI type shere  
    void Awake()
    {
        Instance = this;
    }

    public CanvasGroup GetGroupFromEnum(UIGroupName name) =>
        name switch
        {
            UIGroupName.PLAYER => m_playerHUD,
            UIGroupName.SKILL => m_skillHUD,
            UIGroupName.GAME => m_gameHUD,
            UIGroupName.GAMEFINISHED => m_gameFinishedHUD,
            UIGroupName.DEATH => m_deathHUD,
            UIGroupName.PAUSE => m_pauseHUD,
            UIGroupName.UNIT => m_unitHUD,
            _ => throw new ArgumentException("no valid case: " + name)
        };


    public void FadeGroup(bool forward, float time, List<UIGroupName> names)
    {
        foreach (var name in names)
        {
            StartCoroutine(Fade(forward, time, name));
        }
    }

    IEnumerator Fade(bool forward, float time, UIGroupName name)
    {
        float duration = time;
        float etime = 0;

        while(etime < duration)
        {
            float alpha = Mathf.Lerp(forward ? 0 : 1, forward ? 1 : 0, etime / duration);
            etime += Time.deltaTime;
            GetGroupFromEnum(name).alpha = alpha;

            yield return new WaitForEndOfFrame();
        }
        
        GetGroupFromEnum(name).alpha = forward ? 1 : 0;
        GetGroupFromEnum(name).interactable = forward;
        GetGroupFromEnum(name).blocksRaycasts = forward;
    }
}
public enum UIGroupName
{
    PLAYER,
    SKILL,
    GAME,
    GAMEFINISHED,
    DEATH,
    PAUSE,
    UNIT
}
