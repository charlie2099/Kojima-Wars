using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameStateData", menuName = "Scriptable Objects/GameStateData")]
public class GameStateDataSO : ScriptableObject
{
    [Tooltip("The maximum length of the game in minutes.")]
    public float maxGameLength = 10;

    [Tooltip("The score required for a team to win the game.")]
    public int scoreThreshold = 1000;

    [Tooltip("Whether players can damage their teammates")]
    public bool friendlyFire = false;

    [Header("Capture Zone Values")]
    [Tooltip("The rate the capture percentage increases or coolsdown with each player.")]
     public float perPlayerCaptureRate = 0.5f;

    [Tooltip("The rate the capture percentage increases or coolsdown with each unit.")]
    public float perUnitCaptureRate = 0.01f;

    [Tooltip("The base rate the capture percentage cooldowns with no contesting entities in the zone.")]
    public float contestingCooldownRate = 0.05f;
}
