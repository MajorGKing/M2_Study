using Data;
using Google.Protobuf.Protocol;
using System;

public class Item
{
    public ItemInfo Info { get; set; } = new ItemInfo();

    public long ItemDbId
    {
        get { return Info.ItemDbId; }
    }

    public int TemplateId
    {
        get { return Info.TemplateId; }
        set { Info.TemplateId = value; }
    }

    public int Count
    {
        get { return Info.Count; }
        set { Info.Count = value; }
    }

    public EItemSlotType ItemSlotType
    {
        get { return Info.ItemSlotType; }
        set { Info.ItemSlotType = value; }
    }

    public int OwnerId { get; set; }

    public ItemData TemplateData
    {
        get
        {
            return Managers.Data.ItemDict[TemplateId];
        }
    }

    public EItemType ItemType { get { return TemplateData.Type; } }
    public EItemSubType SubType { get { return TemplateData.SubType; } }
    public int MaxStack { get { return TemplateData.MaxStack; } }

    protected Item(int templateId)
    {
        TemplateId = templateId;
    }

    public static Item MakeItem(ItemInfo itemInfo)
    {
        Managers.Data.ItemDict.TryGetValue(itemInfo.TemplateId, out ItemData itemData);
        if (itemData == null)
            return null;

        Item item = null;

        switch (itemData.Type)
        {
            case EItemType.Equipment:
                item = new Equipment(itemInfo.TemplateId);
                break;
            case EItemType.Consumable:
                item = new Consumable(itemInfo.TemplateId);
                break;
        }

        if (item != null)
        {
            item.Info.MergeFrom(itemInfo);
        }

        return item;
    }

    public static EItemStatus GetItemStatus(EItemSlotType itemSlotType)
    {
        if (EItemSlotType.None < itemSlotType && itemSlotType < EItemSlotType.EquipmentMax)
            return EItemStatus.Equipped;

        if (itemSlotType == EItemSlotType.Inventory)
            return EItemStatus.Inventory;

        if (itemSlotType == EItemSlotType.Warehouse)
            return EItemStatus.Warehouse;

        return EItemStatus.None;
    }

    public EItemStatus GetItemStatus() { return GetItemStatus(ItemSlotType); }
    public bool IsEquipped() { return GetItemStatus() == EItemStatus.Equipped; }
    public bool IsInInventory() { return GetItemStatus() == EItemStatus.Inventory; }
    public bool IsInWarehouse() { return GetItemStatus() == EItemStatus.Warehouse; }
    public int GetAvailableStackCount() { return Math.Max(0, MaxStack - Count); }

    public virtual bool CanUseItem() { return false; }
}
