using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.IO;

[CreateAssetMenu(fileName = "ClassPreset", menuName = "Scriptable Objects/ClassPreset")]
public class PlayerClassSO : ScriptableObject
{
    [Header("Icon")]
    [SerializeField] private Sprite classIcon = default;
    [SerializeField] private Color classColour = default;

    [Header("Preset Class Values")]
    [SerializeField] private string className = default;
    [SerializeField] private string classDescription = default;
    [SerializeField] private int classID = default;

    [Space]
    [SerializeField] private WeaponStats weapon = default;

    [Space]
    [SerializeField] private AbilitySO ability1 = default;
    [SerializeField] private AbilitySO ability2 = default;
    [SerializeField] private AbilitySO ability3 = default;

    [Space]
    [SerializeField] private AbilitySO altAbility1 = default;
    [SerializeField] private AbilitySO altAbility2 = default;
    [SerializeField] private AbilitySO altAbility3 = default;

    private bool useAltAbilities = false;

    private static Dictionary<int,PlayerClassSO> classList = default;
    private static int count = 0;

    private void OnEnable()
    {       
        // create the map if one doesnt exist
        if(classList == null) classList = new Dictionary<int, PlayerClassSO>(); 

        // add this to the list
        classList.Add(classID, this);
    }

    public int GetUniqueID() => classID;
    public static PlayerClassSO GetClassFromID(int id) => classList[id];

    public AbilitySO GetAbility(int id)
    {
        if(IsUsingAltAbilities())
        {
            return id switch
            {
                0 => altAbility1,
                1 => altAbility2,
                2 => altAbility3,
                _ => null,
            };
        }
        else
        {
            return id switch
            {
                0 => ability1,
                1 => ability2,
                2 => ability3,
                _ => null,
            };
        }
    }

    public AbilitySO GetAbilityForInfo(int id)
    {
        return id switch
        {
            0 => ability1,
            1 => ability2,
            2 => ability3,
            _ => null,
        };
    }

    public AbilitySO GetAltAbilityForInfo(int id)
    {
        return id switch
        {
            0 => altAbility1,
            1 => altAbility2,
            2 => altAbility3,
            _ => null,
        };
    }

    public void SetUseAltAbility(bool use)
    {
        useAltAbilities = use;
    }

    // i have added this to get the required weapon information for the UI
    public WeaponStats GetWeapon() => weapon;
    public Color GetClassColour() => classColour;
    public bool IsUsingAltAbilities() => useAltAbilities;
    public string GetClassName() => className;

}
