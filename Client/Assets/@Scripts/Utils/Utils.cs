﻿using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using static Define;
using Object = UnityEngine.Object;

public static class Utils
{
    public static long TickCount { get { return (long)(Time.time * 1000); } }

    public static T GetOrAddComponent<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
            component = go.AddComponent<T>();
        return component;
    }

    public static T FindAncestor<T>(GameObject go) where T : Object
    {
        Transform t = go.transform;
        while (t != null)
        {
            T component = t.GetComponent<T>();
            if (component != null)
                return component;
            t = t.parent;
        }
        return null;
    }

    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (transform == null)
            return null;

        return transform.gameObject;
    }

    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : Object
    {
        if (go == null)
            return null;

        if (recursive == false)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(i);
                if (string.IsNullOrEmpty(name) || transform.name == name)
                {
                    T component = transform.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }
        }
        else
        {
            foreach (T component in go.GetComponentsInChildren<T>())
            {
                if (string.IsNullOrEmpty(name) || component.name == name)
                    return component;
            }
        }

        return null;
    }

    public static Color HexToColor(string color)
    {
        Color parsedColor;

        if (color.Contains("#") == false)
            ColorUtility.TryParseHtmlString("#" + color, out parsedColor);
        else
            ColorUtility.TryParseHtmlString(color, out parsedColor);

        return parsedColor;
    }

    // Animator 컴포넌트 내에 특정 애니메이션 클립이 존재하는지 확인하는 함수
    public static bool HasAnimationClip(Animator animator, string clipName)
    {
        if (animator.runtimeAnimatorController == null)
        {
            return false;
        }

        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
            {
                return true;
            }
        }

        return false;
    }

    public static T ParseEnum<T>(string value)
    {
        return (T)Enum.Parse(typeof(T), value, true);
    }

    public static IPAddress GetIpv4Address(string hostAddress)
    {
        IPAddress[] ipAddr = Dns.GetHostAddresses(hostAddress);

        if (ipAddr.Length == 0)
        {
            Debug.LogError("AuthServer DNS Failed");
            return null;
        }

        foreach (IPAddress ip in ipAddr)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip;
            }
        }

        Debug.LogError("AuthServer IPv4 Failed");
        return null;
    }

    // [OBJ_TYPE(4)][TEMPLATE_ID(8)][ID(20)]
    public static EGameObjectType GetObjectTypeFromId(int id)
    {
        int type = (id >> 28) & 0x0F;
        return (EGameObjectType)type;
    }

    public static int GetTemplateIdFromId(int id)
    {
        int templateId = (id >> 20) & 0xFF;
        return templateId;
    }

    public static Dictionary<EItemSubType, EItemSlotType> SubTypeToEquipTypeMap = new Dictionary<EItemSubType, EItemSlotType>()
    {
        { EItemSubType.Mainweapon,  EItemSlotType.Mainweapon },
        { EItemSubType.Subweapon,   EItemSlotType.Subweapon} ,
        { EItemSubType.Helmet,      EItemSlotType.Helmet },
        { EItemSubType.Chest,       EItemSlotType.Chest },
        { EItemSubType.Leg,         EItemSlotType.Leg },
        { EItemSubType.Shoes,       EItemSlotType.Shoes },
        { EItemSubType.Gloves,      EItemSlotType.Gloves },
        { EItemSubType.Shoulder,    EItemSlotType.Shoulder },
        { EItemSubType.Ring,        EItemSlotType.Ring },
        { EItemSubType.Amulet,      EItemSlotType.Amulet },
    };

    public static EItemSlotType GetEquipSlotType(EItemSubType subType)
    {
        if (SubTypeToEquipTypeMap.TryGetValue(subType, out EItemSlotType value))
            return value;

        return EItemSlotType.None;
    }

    static int _counter = 1;
    public static int GenerateId(EGameObjectType type, int templateId)
    {
        return ((int)type << 28) | (templateId << 20) | (_counter++);
    }
}