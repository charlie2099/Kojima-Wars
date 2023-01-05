using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameFinishedUIController : MonoBehaviour
{
    [SerializeField] private GameObject gameOverContainer;
    [SerializeField] private TMP_Text winnerText;

    public void OnGameEnd(WinCondition condition, string winningTeamName)
    {
        // Hide the rest of the games UI elements
        UIManager.Instance.FadeGroup(false, 0.25f, new List<UIGroupName>() {UIGroupName.GAME, UIGroupName.PLAYER,
            UIGroupName.PAUSE, UIGroupName.DEATH, UIGroupName.UNIT, UIGroupName.SKILL}); 

        // Fade in the game finished UI
        UIManager.Instance.FadeGroup(true, 0.25f, new List<UIGroupName>() { UIGroupName.GAMEFINISHED});

        // unlock cursor
        CursorManager.EnableCursor("game-end-ui");

        // activeate game over container and assign winning team name
        gameOverContainer.SetActive(true);

        switch(condition)
        {
            case WinCondition.SCORE:
                winnerText.text = winningTeamName.ToUpper() + " TEAM HAS REACHED THE SCORE LIMIT";
                break;
            case WinCondition.BASES:
                winnerText.text = winningTeamName.ToUpper() + " TEAM HAS CAPTURED ALL BASES";
                break;
            case WinCondition.TIMER:
                winnerText.text = winningTeamName.ToUpper() + " TEAM HAS THE MOST POINTS WITHIN THE TIME LIMIT";
                break;
        }
    }

    private void Awake()
    {
        gameOverContainer.SetActive(false);
    }

    private void OnDisable()
    {
        CursorManager.DisableCursor("game-end-ui");
    }
}
