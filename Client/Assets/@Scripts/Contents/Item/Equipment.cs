using Data;
using Google.Protobuf.Protocol;
using System;
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

    // ������ �������� ���� ���� ��ȣ.
    public EItemSlotType GetEquipSlotType()
    {
        return Utils.GetEquipSlotType(SubType);
    }

    public void Equip(InventoryManager inventory)
    {
        if (IsInInventory() == false)
            return;

        EItemSlotType equipSlotType = GetEquipSlotType();

        // 0. ���� ������ �̹� ���� �������� �ִٸ� �ϴ� ���´�.
        if (inventory.EquippedItems.TryGetValue(equipSlotType, out Equipment prev))
        {
            if (prev == this)
                return;

            prev.UnEquip(inventory);
        }

        // 1. �κ��丮���� ����.
        inventory.InventoryItems.Remove(ItemDbId);

        // 2. ���� �����ۿ� �߰�.
        inventory.EquippedItems[equipSlotType] = this;

        // 3. ���� ����.
        ItemSlotType = equipSlotType;
    }

    public void UnEquip(InventoryManager inventory)
    {
        if (IsEquipped() == false)
            return;

        EItemSlotType equipSlotType = GetEquipSlotType();
        if (equipSlotType != ItemSlotType)
            return;

        // 1. ���� �����ۿ��� ����.
        inventory.EquippedItems.Remove(equipSlotType);

        // 2. �κ��丮�� �߰�.
        inventory.InventoryItems.Add(ItemDbId, this);

        // 3. ���� ����.
        ItemSlotType = EItemSlotType.Inventory;
    }
}