using Data;
using Google.Protobuf.Protocol;
using System;
using static Define;
using EquipmentData = Data.EquipmentData;

public class Consumable : Item
{
    public ConsumableData ConsumableData { get; private set; }
    public EffectData EffectData { get; private set; }
    public EConsumableGroupType ConsumableGroupType { get { return ConsumableData.ConsumableGroupType; } }
    public int CoolTime { get { return ConsumableData.CoolTime; } }

    long _nextUseTick = 0;

    public Consumable(int templateId) : base(templateId)
    {
        Init();
    }

    void Init()
    {
        if (TemplateData == null)
            return;

        if (TemplateData.Type != EItemType.Consumable)
            return;

        ConsumableData = (ConsumableData)TemplateData;
        {
            EffectData = ConsumableData.EffectData;
        }
    }

    #region 소모품/쿨타임

    public override bool CanUseItem()
    {
        // 1. 수량 확인.
        if (Count <= 0)
            return false;

        // 2. 쿨타임 확인.
        if (GetRemainingCooltimeInTicks() > 0)
            return false;

        return true;
    }

    public void UpdateCooltime(long remainingTicks)
    {
        long nextUseTick = Utils.TickCount + remainingTicks;

        // 그룹 쿨타임 적용.
        if (ConsumableGroupType != EConsumableGroupType.None)
            Managers.Inventory.ItemGroupCooltimeDic[ConsumableGroupType] = nextUseTick;
        else
            _nextUseTick = nextUseTick;

        // 2. 이벤트 전파.
        Managers.Event.TriggerEvent(EEventType.InventoryChanged);
    }

    public int GetRemainingCooltimeInTicks()
    {
        // 그룹 쿨타임 적용.
        if (ConsumableGroupType != EConsumableGroupType.None)
            return Managers.Inventory.GetRemainingItemGroupCooltimeInTicks(ConsumableGroupType);

        return (int)Math.Max(0, _nextUseTick - Utils.TickCount);
    }

    public float GetRemainingCooltimeInSeconds()
    {
        return GetRemainingCooltimeInTicks() / 1000.0f;
    }

    public float GetRemainingCoolTimeRatio()
    {
        if (CoolTime == 0)
            return 0;

        return GetRemainingCooltimeInSeconds() / CoolTime;
    }

    #endregion
}
