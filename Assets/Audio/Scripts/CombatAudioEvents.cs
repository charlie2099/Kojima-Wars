using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Create Combat Audio Data", fileName = "CombatAudioData")]
public class CombatAudioEvents : ScriptableObject
{
    [Header("Shots")]
    [FMODUnity.EventRef]
    public string pistolShot;
    [FMODUnity.EventRef]
    public string silencedShot;
    [FMODUnity.EventRef]
    public string minigunShot;
    [FMODUnity.EventRef]
    public string assaultRifleShot;
    [FMODUnity.EventRef]
    public string burstRifleShot;
    [FMODUnity.EventRef]
    public string submachineGunShot;
    [FMODUnity.EventRef]
    public string sniperShot;


    [Header("Reloading")]
    [FMODUnity.EventRef]
    public string pistolReload;
    [FMODUnity.EventRef]
    public string minigunReload;
    [FMODUnity.EventRef]
    public string assaultRifleReload;
    [FMODUnity.EventRef]
    public string burstRifleReload;
    [FMODUnity.EventRef]
    public string submachineGunReload;
    [FMODUnity.EventRef]
    public string sniperReload;


    [Header("Shield")]
    [FMODUnity.EventRef]
    public string shieldRegen;
    [FMODUnity.EventRef]
    public string shieldImpact;
    [FMODUnity.EventRef]
    public string shieldDeploy;


    [Header("General")]
    [FMODUnity.EventRef]
    public string weaponSwap;
    [FMODUnity.EventRef]
    public string toss;
    [FMODUnity.EventRef]
    public string flames;
    [FMODUnity.EventRef]
    public string explosion;
    [FMODUnity.EventRef]
    public string laser;
    [FMODUnity.EventRef]
    public string pulse;
    [FMODUnity.EventRef]
    public string beep;
    [FMODUnity.EventRef]
    public string buzz;
    [FMODUnity.EventRef]
    public string spark;
    [FMODUnity.EventRef]
    public string radioStatic;
    [FMODUnity.EventRef]
    public string poweringUp;


    [Header("Abilities")]
    [FMODUnity.EventRef]
    public string plasma;
    [FMODUnity.EventRef]
    public string plasmaCharge;
    [FMODUnity.EventRef]
    public string flashBang;
    [FMODUnity.EventRef]
    public string EMPShot;
    [FMODUnity.EventRef]
    public string EMPCharge;
    [FMODUnity.EventRef]
    public string recall;
    [FMODUnity.EventRef]
    public string slam;
    [FMODUnity.EventRef]
    public string UAV;


    [Header("Turrets")]
    [FMODUnity.EventRef]
    public string turretPlaced;
    [FMODUnity.EventRef]
    public string turretFire;
    [FMODUnity.EventRef]
    public string turretTurn;
    [FMODUnity.EventRef]
    public string turretHit;
}
