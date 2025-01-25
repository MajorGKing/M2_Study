using System;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf.Protocol;
using System.Linq;
using Data;
using static Define;

public class InventoryManager
{
    public const int DEFAULT_INVENTORY_SLOT_COUNT = 50;

    // 소유한 모든 아이템.
    public Dictionary<long/*itemDbId*/, Item> AllItems { get; private set; } = new Dictionary<long, Item>();

    // 캐싱.
    public Dictionary<EItemSlotType, Equipment> EquippedItems = new Dictionary<EItemSlotType, Equipment>();
    public Dictionary<long/*itemDbId*/, Item> WarehouseItems { get; } = new Dictionary<long, Item>();
    public Dictionary<long/*itemDbId*/, Item> InventoryItems { get; } = new Dictionary<long, Item>();

    public List<Item> QuickSlotItems = new List<Item>();
    public long Gold { get; private set; } = 0;
    public long Gem { get; private set; } = 0;

    public Dictionary<EConsumableGroupType, long/*nextUseTick*/> ItemGroupCooltimeDic = new Dictionary<EConsumableGroupType, long>();

    public int ItemCountInInventory { get { return InventoryItems.Count; } }

    #region 추가 / 갱신 / 삭제
    public void Add(ItemInfo itemInfo, bool triggerEvent = true)
    {
        Item item = Item.MakeItem(itemInfo);
        if (item == null)
            return;

        AllItems.Add(item.ItemDbId, item);

        EItemStatus status = item.GetItemStatus();
        switch (status)
        {
            case EItemStatus.Equipped:
                // DB 요청이 꼬여서 중복 장착 되었다면 인벤토리로 보낸다.
                if (EquippedItems.TryAdd(item.ItemSlotType, (Equipment)item) == false)
                {
                    item.ItemSlotType = EItemSlotType.Inventory;
                    InventoryItems.Add(item.ItemDbId, item);
                }
                break;
            case EItemStatus.Inventory:
                InventoryItems.Add(item.ItemDbId, item);
                break;
            case EItemStatus.Warehouse:
                WarehouseItems.Add(item.ItemDbId, item);
                break;
        }

        // 임시로 자동 등록
        if (item.SubType == EItemSubType.Consumable)
            QuickSlotItems.Add((item));

        if (triggerEvent)
            Managers.Event.TriggerEvent(EEventType.InventoryChanged);
    }

    public void Update(ItemInfo itemInfo)
    {
        Item item = GetItemByDbId(itemInfo.ItemDbId);
        if (item == null)
            return;

        if (item.ItemSlotType != itemInfo.ItemSlotType)
            ChangeItemSlot(item.ItemDbId, itemInfo.ItemSlotType);

        item.Info.MergeFrom(itemInfo);

        Managers.Event.TriggerEvent(EEventType.InventoryChanged);
    }

    public void ChangeItemSlot(long itemDbId, EItemSlotType newSlotType)
    {
        Item item = GetItemByDbId(itemDbId);
        if (item == null)
            return;

        // TODO : 가능한 조합
        // 1. 인벤토리 -> 장착
        // 2. 장착 -> 인벤토리
        // 3. 인벤토리 -> 창고
        // 4. 창고 -> 인벤토리
        // 5. 인벤토리 -> 인벤토리 (X)
        // 6. 창고 -> 창고 (X)

        EItemSlotType prevSlotType = item.ItemSlotType;
        EItemStatus prevStatus = Item.GetItemStatus(prevSlotType);
        EItemStatus newStatus = Item.GetItemStatus(newSlotType);

        // 1. 인벤토리 -> 장착.
        if (prevStatus == EItemStatus.Inventory && newStatus == EItemStatus.Equipped)
        {
            Equipment equipment = item as Equipment;
            if (equipment == null)
                return;

            equipment.Equip(this);
        }
        // 2. 장착 -> 인벤토리.
        else if (prevStatus == EItemStatus.Equipped && newStatus == EItemStatus.Inventory)
        {
            Equipment equipment = item as Equipment;
            if (equipment == null)
                return;

            equipment.UnEquip(this);
        }

        Managers.Event.TriggerEvent(EEventType.InventoryChanged);
    }

    public void Remove(long itemDbId)
    {
        Item item = GetItemByDbId(itemDbId);
        if (item == null)
            return;

        AllItems.Remove(item.ItemDbId);

        EItemStatus status = item.GetItemStatus();
        switch (status)
        {
            case EItemStatus.Equipped:
                EquippedItems.Remove(item.ItemSlotType);
                break;
            case EItemStatus.Inventory:
                InventoryItems.Remove(item.ItemDbId);
                break;
            case EItemStatus.Warehouse:
                WarehouseItems.Remove(item.ItemDbId);
                break;
        }

        QuickSlotItems.Remove(item);

        Managers.Event.TriggerEvent(EEventType.InventoryChanged);
    }

    public void Clear()
    {
        EquippedItems.Clear();
        InventoryItems.Clear();
        Gold = 0;
        Gem = 0;
    }
    #endregion

    #region Req

    public void ReqEquipItem(long itemDbId)
    {
        Equipment item = GetItemByDbId(itemDbId) as Equipment;
        if (item == null)
        {
            Debug.Log("@아이템 존재 안함");
            return;
        }

        C_EquipItem pkt = new C_EquipItem();
        pkt.ItemDbId = itemDbId;
        Managers.Network.GameServer.Send(pkt);
    }

    public void ReqUnEquipItem(long itemDbId)
    {
        var item = EquippedItems.Values.FirstOrDefault(x => x.ItemDbId == itemDbId);
        if (item == null)
        {
            Debug.Log("아이템존재안함");
            return;
        }

        if (IsInventoryFull())
        {
            Debug.Log("인벤꽉참");
            return;
        }

        C_UnEquipItem pkt = new C_UnEquipItem();
        pkt.ItemDbId = itemDbId;
        Managers.Network.GameServer.Send(pkt);
    }

    public void ReqUseItem(Item item)
    {
        C_UseItem sendpkt = new C_UseItem() { ItemDbId = item.ItemDbId };
        Managers.Network.GameServer.Send(sendpkt);
    }

    public void ReqDeleteItem(long itemDbId)
    {
        // TODO UI나오면 겹치는아이템은 Count만 내릴 수 있게 조절
        Item item = GetItemByDbId(itemDbId);
        if (item == null)
            return;

        C_DeleteItem pkt = new C_DeleteItem();
        pkt.ItemDbId = itemDbId;
        Managers.Network.GameServer.Send(pkt);
    }

    #endregion

    #region Handler

    public void HandleEnterGame(S_EnterGame packet)
    {
        AllItems.Clear();
        EquippedItems.Clear();
        InventoryItems.Clear();
        WarehouseItems.Clear();
        QuickSlotItems.Clear();

        foreach (ItemInfo itemInfo in packet.Items)
            Add(itemInfo, triggerEvent: false);

        Managers.Event.TriggerEvent(EEventType.InventoryChanged);
    }

    public void HandleUseItem(S_UseItem packet)
    {
        Item item = GetItemByDbId(packet.ItemDbId);
        if (item == null)
        {
            Debug.LogError($"HandleUseItemResult() >> 아이템 없음");
            return;
        }

        UpdateCooltime(item.ItemDbId, packet.RemainingTicks);
    }

    #endregion

    #region 쿨타임 관리

    public bool CanUseItem(long itemDbId)
    {
        Consumable item = GetItemByDbId(itemDbId) as Consumable;
        if (item == null)
            return false;

        return item.CanUseItem();
    }

    public void UpdateCooltime(long itemDbId, long remainingTicks)
    {
        Consumable item = GetItemByDbId(itemDbId) as Consumable;
        if (item == null)
            return;

        item.UpdateCooltime(remainingTicks);
    }

    public float GetRemainingCoolTimeRatio(long itemDbId)
    {
        Consumable item = GetItemByDbId(itemDbId) as Consumable;
        if (item == null)
            return 0;

        return item.GetRemainingCoolTimeRatio();
    }

    public int GetRemainingItemGroupCooltimeInTicks(EConsumableGroupType type)
    {
        if (ItemGroupCooltimeDic.TryGetValue(type, out long nextUseTick) == false)
            return 0;

        return (int)Math.Max(0, nextUseTick - Utils.TickCount);
    }

    public float GetRemainingItemGroupCooltimeInSeconds(EConsumableGroupType type)
    {
        return GetRemainingItemGroupCooltimeInTicks(type) / 1000.0f;
    }

    #endregion

    #region  Helper
    public Item GetItemByDbId(long itemDbId)
    {
        AllItems.TryGetValue(itemDbId, out Item item);
        return item;
    }

    public Equipment GetEquippedItem(EItemSlotType itemSlotType)
    {
        EquippedItems.TryGetValue(itemSlotType, out Equipment item);
        return item;
    }

    public Item GetEquippedItem(long itemDbId)
    {
        Item item = GetItemByDbId(itemDbId);
        if (item == null)
            return null;

        if (item.IsEquipped() == false)
            return null;

        return item;
    }

    public bool IsEquippedItem(long instanceId)
    {
        return GetEquippedItem(instanceId) != null;
    }

    public bool IsEquippedItem(EItemSlotType itemSlotType)
    {
        return GetEquippedItem(itemSlotType) != null;
    }

    public Item GetEquippedItemBySubType(EItemSubType subType)
    {
        return EquippedItems.Values.FirstOrDefault(x => x.SubType == subType);
    }

    public Item GetInventoryItemByDbId(long itemDbId)
    {
        InventoryItems.TryGetValue(itemDbId, out Item item);
        return item;
    }

    public List<Item> GetItemsByGroupTypeInInventory(EItemType type)
    {
        List<Item> list = GetAllItemsInInventory().Where(x => x.TemplateData.Type == type).ToList();
        list.AddRange(EquippedItems.Values);

        return list;
    }

    public List<Item> GetEtcItemsInInventory()
    {
        List<Item> list = GetAllItemsInInventory()
            .Where(x => x.TemplateData.Type == EItemType.Equipment && x.TemplateData.Type == EItemType.Consumable).ToList();

        return list;
    }

    public List<Item> GetConsumableItemsInInventory()
    {
        List<Item> list = GetAllItemsInInventory()
            .Where(x => x.TemplateData.SubType == EItemSubType.Consumable).ToList();

        return list;
    }

    public List<Item> GetAllItems()
    {
        return AllItems.Values.ToList();
    }

    public List<Equipment> GetEquippedItems()
    {
        return EquippedItems.Values.ToList();
    }

    public List<ItemInfo> GetEquippedItemInfos()
    {
        return EquippedItems.Values.Select(x => x.Info).ToList();
    }

    public List<Item> GetAllItemsInInventory()
    {
        return InventoryItems.Values.ToList();
    }

    public bool IsInventoryFull()
    {
        return InventoryItems.Count >= GetInventorySlotCount();
    }

    public int GetInventorySlotCount()
    {
        //TODO 계산
        //return DEFAULT_INVENTORY_SLOT_COUNT + Add;
        return DEFAULT_INVENTORY_SLOT_COUNT;
    }

    #endregion

}
