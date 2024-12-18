using System.Collections;
using System.Collections.Generic;
using Data;
using UnityEngine;

public class Hero : Creature
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    // TEMP : ���ǿ� �������� �ڵ�. ��ġ�� �κ� ��� ������ �÷����� ����.
    protected override void Update()
    {
        // FSM ����� ���� ����.
        UpdateAI();

        // �⺻������ ��� ��ü�� ĭ ������ ����������, Ŭ�󿡼� '������' �����̴� ���� ó���� ���ش�.
        UpdateLerpToCellPos(MoveSpeed, true);
    }

    protected override void UpdateMove()
    {
        base.UpdateMove();
    }

    public virtual void SetInfo(int templateId)
    {
        if (Managers.Data.HeroDic.TryGetValue(templateId, out HeroData heroData) == false)
            return;

        SetSpineAnimation(heroData.SkeletonDataId, SortingLayers.HERO, "SkeletonAnimation");
    }
}
