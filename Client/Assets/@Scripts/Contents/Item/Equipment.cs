using Data;
using Google.Protobuf.Protocol;
using EquipmentData = Data.EquipmentData;

public class Equipment : Item
{
    public EItemSubType EquipType { get; private set; }
    public EffectData EffectData;

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

        EquipmentData data = (EquipmentData)TemplateData;
        {
            EquipType = data.SubType;
            EffectData = data.EffectData;
        }
    }

    // 아이템 장착했을 때의 슬롯 번호.
    public EItemSlotType GetEquipSlotType()
    {
        return Utils.GetEquipSlotType(SubType);
    }

    public void Equip(InventoryManager inventory)
    {
        if (IsInInventory() == false)
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
    }

    public void UnEquip(InventoryManager inventory)
    {
        if (IsEquipped() == false)
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
    }
}