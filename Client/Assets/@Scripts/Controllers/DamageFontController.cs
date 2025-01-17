using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using Scripts.Data.SO;
using UnityEngine;

public class DamageFontInfo
{
    public float Damage;
    public Transform Parent;
    public EDamageType DamageType;
}

public class DamageFontController : MonoBehaviour
{
    private Queue<DamageFontInfo> _infos = new Queue<DamageFontInfo>();

    public void OnEnable()
    {
        _infos.Clear();
        StartCoroutine(CoUpdate());
    }

    IEnumerator CoUpdate()
    {
        WaitForSeconds wait = new WaitForSeconds(0.3f);
        while (true)
        {
            if (_infos.Count > 0)
            {
                DamageFontInfo info = _infos.Dequeue();
                Spawn(info);
            }
            yield return wait;
        }
    }

    public void AddDamageFont(float damage, Transform parent, EDamageType damageType)
    {
        DamageFontInfo info = new DamageFontInfo()
        {
            Damage = damage,
            Parent = parent,
            DamageType = damageType
        };
        _infos.Enqueue(info);
    }

    private void Spawn(DamageFontInfo info)
    {
        string prefabName = "DamageFont";
        GameObject go = Managers.Resource.Instantiate(prefabName, pooling: true);
        DamageFont damageText = go.GetComponent<DamageFont>();
        damageText.SetInfo(info.Damage, info.Parent, info.DamageType);
    }
}
