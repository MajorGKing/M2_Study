using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : Item
{
    public CollectibleData CollectibleData { get; private set; }
    
    public Collectible(int templateId) : base(templateId)
    {
        Init();
    }

    void Init()
    {
        if (TemplateData == null)
            return;

        if (TemplateData.Type != EItemType.Collectible)
            return;

        CollectibleData = (CollectibleData)TemplateData;
    }
}
