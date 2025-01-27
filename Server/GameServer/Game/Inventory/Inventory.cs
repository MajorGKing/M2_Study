using Google.Protobuf.Protocol;
using Server.Data;
using GameServer;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net.Sockets;

namespace Server.Game
{
    public class InventoryComponent
    {
        public readonly int DEFAULT_SLOT_COUNT = 30;

        public Hero Owner { get; set; }

        // 소유한 모든 아이템.
        public Dictionary<long/*itemDbId*/, Item> AllItems = new Dictionary<long, Item>();

        // 캐싱.
        public Dictionary<EItemSlotType, Equipment> EquippedItems { get; } = new Dictionary<EItemSlotType, Equipment>();
        public Dictionary<long/*itemDbId*/, Item> WarehouseItems { get; } = new Dictionary<long, Item>();
        public Dictionary<long/*itemDbId*/, Item> InventoryItems { get; } = new Dictionary<long, Item>();

        public Dictionary<EConsumableGroupType, long/*nextUseTick*/> ItemGroupCooltimeDic = new Dictionary<EConsumableGroupType, long>();

        public InventoryComponent(Hero owner)
        {
            Owner = owner;
        }

        public void Init(List<ItemDb> allItems)
        {
            foreach(ItemDb itemDb in allItems)
            {
                Item item = Item.MakeItem(itemDb);
                if (item == null)
                    continue;

                Add(item);
            }
        }

        #region 추가/삭제
        public void Add(Item item, bool sendToClient = false)
        {
            AllItems.Add(item.ItemDbId, item);

            EItemStatus status = item.GetItemStatus();
            switch (status)
            {
                case EItemStatus.Equipped:
                    // DB 요청이 꼬여서 중복 장착 되었다면 인벤토리로 보낸다.
                    if(EquippedItems.TryAdd(item.ItemSlotType, (Equipment)item) == false)
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

            if (sendToClient)
                item.SendAddPacket(Owner);
        }

        public void AddCount(long itemDbId, int count, bool sendToClient = false)
        {
            Item item = GetItemByDbId(itemDbId);
            if (item == null)
                return;

            item.AddCount(Owner, count, sendToClient);
        }

        public void Remove(Item item, bool sendToClient = false)
        {
            AllItems.Remove(item.ItemDbId);

            EItemStatus status  = item.GetItemStatus();
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

            if(sendToClient)
                item.SendDeletePacket(Owner);
        }
        #endregion

        #region 장착/해제
        public bool EquipItem(long itemDbId)
        {
            Equipment item = GetItemByDbId(itemDbId) as Equipment;
            if(item == null) 
                return false;

            EItemSlotType itemSlotType = Utils.GetEquipSlotType(item.SubType);

            // 1. 같은 부위에 장착중인 템이 있는 경우, 그 아이템 장착 해제.
            if (EquippedItems.TryGetValue(itemSlotType, out Equipment prev))
            {
                if (prev == item)
                    return false;

                prev.UnEquip(this);
            }

            // 2. 아이템 장착.
            item.Equip(this);

            return true;
        }

        public bool UnEquipItem(long itemDbId)
        {
            Equipment item = GetItemByDbId(itemDbId) as Equipment;
            if (item == null)
                return false;

            // 1. 아이템 장착 해제.
            item.UnEquip(this);

            return true;
        }

        public void ApplyEquipmentEffects()
        {
            foreach (Equipment item in EquippedItems.Values)
            {
                // 장착한 아이템 이펙트 적용
                if (item.EffectData == null)
                    continue;

                Owner.EffectComp.ApplyEffect(item.EffectData, Owner, send: false);
            }
        }
        #endregion

        #region 소모품/쿨타임
        /*
         * 1.그룹별 쿨타임이 있는 아이템
         *   포션끼리 쿨타임 공유
         *   - 체력,마력포션(소,중,대)
         *   - 공속,이속 물약(1단계, 2단계, 3단계)
         *   등
         * 2. 단일 쿨타임 아이템(TODO)
         *   
         */

        public int UpdateCooltime(long itemDbId)
        {
            Consumable item = GetItemByDbId(itemDbId) as Consumable;
            if (item == null)
                return 0;

            return item.UpdateCooltime(this);
        }

        public int GetRemainingItemGroupCooltimeInTicks(EConsumableGroupType type)
        {
            if (ItemGroupCooltimeDic.TryGetValue(type, out long nextUseTick) == false)
                return 0;

            return (int)Math.Max(0, nextUseTick - Utils.TickCount);
        }
        #endregion

        #region Handler
        public void HandleUseItem(long itemDbId)
        {
            Item item = Owner.Inven.GetItemByDbId(itemDbId);
            if (item == null)
                return;

            if (item.CanUseItem(this) == false)
                return;

            DBManager.UseItem(Owner, item);
        }

        public void HandleDeleteItem(long itemDbId)
        {
            Item item = GetInventoryItemByDbId(itemDbId);
            if (item == null)
                return;

            DBManager.DeleteItem(Owner, item);
        }
        #endregion

        #region Helper
        public bool IsInventoryFull()
        {
            return InventoryItems.Count >= DEFAULT_SLOT_COUNT;
        }

        public Item GetItemByDbId(long itemDbId)
        {
            AllItems.TryGetValue(itemDbId, out Item item);
            return item;
        }

        public Item GetEquippedItemByDbId(long itemDbId)
        {
            return EquippedItems.Values.Where(i => i.ItemDbId == itemDbId).FirstOrDefault();
        }

        public Item GetInventoryItemByDbId(long itemDbId)
        {
            InventoryItems.TryGetValue(itemDbId, out Item item);
            return item;
        }

        public Item GetAnyInventoryItemByCondition(Func<Item, bool> condition)
        {
            return InventoryItems.Values.Where(condition).FirstOrDefault();
        }

        public List<ItemInfo> GetAllItemInfos()
        {
            return AllItems.Values.Select(i => i.Info).ToList();
        }

        #endregion

    }
}
