using System;
using Unity.Netcode;
using UnityEngine;

public class AbilitySO : ScriptableObject
{
    [Header("Ability Name and Description")]
    [SerializeField] private string abilityName = default;
    [TextArea][SerializeField] private string abilityDescription = default;

    public string GetAbilityName() => abilityName;
    public string GetAbilityDescription() => abilityDescription;

    [Header("Ability Timings")]
    [SerializeField] private float cooldownTime;
    [SerializeField] private float activeTime;
    
    public float CooldownTime
    {
        private get => _coolDownData;
        set => _coolDownData = value;
    }
    private float _coolDownData;

    void OnEnable()
    {
        _coolDownData = cooldownTime;
    }

    public float GetCooldownTime() => CooldownTime;
    public float GetActiveTime() => activeTime;

    [Header("Ability Icon")]
    [SerializeField] private Sprite abilityIcon = default;
    public Sprite GetIconSprite() => abilityIcon;


    [Header("Ability Prefab")]
    [SerializeField] private GameObject abilityPrefab = default;
    [SerializeField] private EAbilities abilityType = default;
    public GameObject GetAbilityPrefab() => abilityPrefab;
    public EAbilities GetAbilityType() => abilityType;

    public virtual void CastAbility(Entity caster, int id) 
    {
        AbilityPrefabSpawner.SpawnAbilityPrefab(id, caster.gameObject);
    }
}
