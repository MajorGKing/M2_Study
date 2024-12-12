using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public static class Extension
{
    public static T GetOrAddComponent<T>(this GameObject go) where T : Component
    {
        return Utils.GetOrAddComponent<T>(go);
    }

    public static void BindEvent(this GameObject go, Action<PointerEventData> action = null, Define.ETouchEvent type = Define.ETouchEvent.Click)
    {
        UI_Base.BindEvent(go, action, type);
    }

    public static bool IsValid(this GameObject go)
    {
        return go != null && go.activeSelf;
    }
   
    public static void MakeMask(this ref LayerMask mask, List<Define.ELayer> list)
    {
        foreach (Define.ELayer layer in list)
            mask |= (1 << (int)layer);
    }

    public static void AddLayer(this ref LayerMask mask, Define.ELayer layer)
    {
        mask |= (1 << (int)layer);
    }

    public static void RemoveLayer(this ref LayerMask mask, Define.ELayer layer)
    {
        mask &= ~(1 << (int)layer);
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            (list[k], list[n]) = (list[n], list[k]);//swap
        }
    }
}
