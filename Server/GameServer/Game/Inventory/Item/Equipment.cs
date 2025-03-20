using Google.Protobuf.Protocol;
using Server.Data;
using GameServer;
using System.Diagnostics.Metrics;

namespace Server.Game
{
    public class Equipment : Item
    {
        public EItemSubType EquipType { get; private set; }
        public EffectData EffectData { get; private set; }
		public EquipmentData EquipmentData { get; private set; }

        public Equipment(int templateId) : base(templateId)
        {
            Init();
        }

        void Init()
        {
            if (TemplateData == null)
                return;

            if (TemplateData.Type != EItemType.Equipment)
                return;

            EquipmentData = (EquipmentData)TemplateData;
            {
                EquipType = EquipmentData.SubType;
                EffectData = EquipmentData.EffectData;
            }
        }

        public void ApplyEnchantLevel(InventoryComponent inventory, bool sendToClient = false)
        {
            if (EquipmentData.NextLevelItem == null)
                return;

            // 아이템 정보 교체 (메모리 적용).
            TemplateId = EquipmentData.NextLevelItem.TemplateId;
            Init();

            // 아이템 정보 교체 패킷 전송.
            if (sendToClient)
            {
                SendUpdatePacket(inventory.Owner, EUpdateItemReason.Enchant);
            }
        }

        // 아이템 장착했을 때의 슬롯 번호.
        public EItemSlotType GetEquipSlotType()
        {
            return Utils.GetEquipSlotType(SubType);
        }

        public void Equip(InventoryComponent inventory)
        {
            if (IsInInventory() == false)
                return;

            Hero owner = inventory.Owner;
            if (owner == null)
                return;

            EItemSlotType equipSlotType = GetEquipSlotType();

            // 0. 같은 부위에 이미 장착 아이템이 있다면 일단 벗는다.
            if (inventory.EquippedItems.TryGetValue(equipSlotType, out Equipment prev))
            {
                if (prev == this)
                    return;

                prev.UnEquip(inventory);
            }

            // 1. 인벤토리에서 제거.
            inventory.InventoryItems.Remove(ItemDbId);

            // 2. 장착 아이템에 추가.
            inventory.EquippedItems[equipSlotType] = this;

            // 3. 슬롯 갱신.
            ItemSlotType = equipSlotType;

            // 4. DB에 Noti.
            DBManager.EquipItemNoti(owner, this);

			// 5. 아이템 보너스 적용
			owner.AddItemBonusStat(EquipmentData);

			// 6. 장착한 아이템 이펙트 적용.
			if (EffectData != null)
				owner.EffectComp.ApplyEffect(EffectData, owner);

			// 7. 패킷전송.
			SendChangeItemSlotPacket(owner);
            owner.SendRefreshStat();
        }

        public void UnEquip(InventoryComponent inventory)
        {
            if (IsEquipped() == false)
                return;

            Hero owner = inventory.Owner;
            if (owner == null)
                return;

            EItemSlotType equipSlotType = GetEquipSlotType();
            if (equipSlotType != ItemSlotType)
                return;

            // 1. 장착 아이템에서 제거.
            inventory.EquippedItems.Remove(equipSlotType);

            // 2. 인벤토리에 추가.
            inventory.InventoryItems.Add(ItemDbId, this);

            // 3. 슬롯 갱신.
            ItemSlotType = EItemSlotType.Inventory;

            // 4. DB에 Noti.
            DBManager.EquipItemNoti(owner, this);

            // 5. 아이템 보너스 삭제
            owner.RemoveItemBonusStat(EquipmentData);

            // 6. 기존 아이템 이펙트 제거.
            if (EffectData != null)
                owner.EffectComp.RemoveItemEffect(EffectData.TemplateId);

			// 7. 패킷전송.
			SendChangeItemSlotPacket(owner);
			owner.SendRefreshStat();
		}

		public bool TryEnchant(InventoryComponent inventory)
        {
			var scroll = inventory.GetEnchantScroll(SubType);
			if (scroll == null || scroll.Count == 0)
				return false;

			int prob = GetEnchantProbability();
			bool success = Utils.CheckProbability(prob);

			// 1. 강화 재료 소모
			DBManager.UseItemNoti(inventory.Owner, scroll);

			// 2. 아이템 강화 or 파괴 진행
			// 강화 성공시 EffectData, TemplateData변경
			if (success)
			{
				DBManager.EnchantSuccessNoti(inventory.Owner, this);
				inventory.Owner.SendSystemEvent(ESystemEventType.EnchantSuccess);
			}	
			else
			{
				DBManager.DeleteItemNoti(inventory.Owner, this);
				inventory.Owner.SendSystemEvent(ESystemEventType.EnchantFail);
			}
			
            return true;
		}

        #region Helpers
        public bool CanEnchant(InventoryComponent inventory)
        {
			// 1. 최대 강화 레벨 확인. 
			if (IsMaxEnchantLevel())
				return false;

			// 2. 장착중이면 리턴
			if (IsEquipped())
				return false;

			// 3. 강화 재료 확인
			if (inventory.GetEnchantScroll(SubType) == null)
				return false;

			return true;
        }

		private int GetEnchantCount()
		{
			return EquipmentData.TemplateId - EquipmentData.BaseItemDataId; 
        }

		private bool IsMaxEnchantLevel()
		{
			return EquipmentData.NextLevelItem == null ? true : false;
		}

		private bool IsSafeToEnchant()
		{
			return GetEnchantCount() < EquipmentData.SafeEnchantLevel;
		}

		private int GetEnchantProbability()
		{
			if (IsSafeToEnchant())
				return 100;

			// TODO : Config
			int enchantCount = GetEnchantCount();
			switch (enchantCount)
			{
				case 6:
					return 70;
				case 7:
					return 50;
				case 8:
					return 30;
				case 9:
					return 20;
				default:
					return 0;
			}
        }
        #endregion
    }
}
