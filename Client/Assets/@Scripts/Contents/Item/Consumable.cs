using Data;
using Google.Protobuf.Protocol;
using System;
using static Define;

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
        // 1. ¼ö·® È®ÀÎ.
        if (Count <= 0)
            return false;

        // 2. ÄðÅ¸ÀÓ È®ÀÎ.
        if (GetRemainingCooltimeInTicks() > 0)
            return false;

        return true;
    }

    public void UpdateCooltime(long remainingTicks)
    {
        long nextUseTick = Utils.TickCount + remainingTicks;

        // ±×·ì ÄðÅ¸ÀÓ Àû¿ë.
        if (ConsumableGroupType != EConsumableGroupType.None)
            Managers.Inventory.ItemGroupCooltimeDic[ConsumableGroupType] = nextUseTick;
        else
            _nextUseTick = nextUseTick;

        // 2. ÀÌº¥Æ® ÀüÆÄ.
        Managers.Event.TriggerEvent(EEventType.InventoryChanged);
    }

    public int GetRemainingCooltimeInTicks()
    {
        // ±×·ì ÄðÅ¸ÀÓ Àû¿ë.
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
