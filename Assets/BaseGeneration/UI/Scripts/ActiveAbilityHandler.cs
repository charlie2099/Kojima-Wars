using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActiveAbilityHandler : MonoBehaviour
{
    private Dictionary<AbilitySO, GameObject> m_objectList = new Dictionary<AbilitySO, GameObject>();
    private PlayerClassSO m_classSo;
    
    public void OnEnable()
    {
        WeaponScript.OnBulletFired += StopActiveAbilityShooting;
    }
    public void OnDisable()
    {
        WeaponScript.OnBulletFired -= StopActiveAbilityShooting;
    }

    public void StartActiveAbility(AbilitySO ability,GameObject obj)
    {
        m_objectList.Add(ability, obj);
        ActiveAbilityUI objectAbility = obj.GetComponent<ActiveAbilityUI>();
        objectAbility.SetIconImage(ability.GetIconSprite());
        
        StartCoroutine(FillProgress(ability,obj, obj.GetComponent<ActiveAbilityUI>().GetFillImage(), ability.GetActiveTime()));
    }

    private IEnumerator FillProgress(AbilitySO key, GameObject obj, Image fillImage, float activeTime)
    {
        float duration = activeTime;
        float etime = 0;

        while(etime < duration)
        {
            float fill = etime / duration;
            if(!fillImage) yield break;
            fillImage.fillAmount = fill;
            etime += Time.deltaTime;
            yield return null;
        }
        
        Destroy(m_objectList[key].gameObject);
        m_objectList.Remove(key);
    }

    //Add other reasons for stop active ability's that require shooting
    public void StopActiveAbilityShooting(int obj)
    {
        List<AbilitySO> keys = new List<AbilitySO>(m_objectList.Keys);
        foreach (var key in keys)
        {
            if (key.GetAbilityType() == EAbilities.CLOAK)
            {
                Destroy(m_objectList[key].gameObject);
                m_objectList.Remove(key);
            }
        }
    }
    
    
}
