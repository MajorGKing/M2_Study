using System.Collections;
using System.Collections.Generic;
using Data;
using Google.Protobuf.Protocol;
using Data;
using Spine;
using UnityEngine;

public class Hero : Creature
{
    protected HeroData HeroData { get; private set; }
    protected SkillData _mainSkill;
    public virtual HeroInfo HeroInfo { get; set; }

    #region LifeCycle
    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        ObjectType = EGameObjectType.Hero;
    }

    protected override void Start()
    {
        base.Start();
    }

    public override void SetInfo(int templateId)
    {
        base.SetInfo(templateId);
        if(Managers.Data.HeroDict.TryGetValue(templateId, out HeroData heroData))
        {
            HeroData = heroData;
        }

        HeroData.SkillMap.TryGetValue(ESkillSlot.Main, out _mainSkill);
    }

    protected override void Update()
    {
        // �⺻������ ��� ��ü�� ĭ ������ ����������, Ŭ�󿡼� '������' �����̴� ���� ó���� ���ش�.
        UpdateLerpToCellPos(MoveSpeed, true);
    }
    #endregion

    #region AI
    protected override void UpdateMove()
    {
        base.UpdateMove();
    }

    protected override void UpdateSkill()
    {
        base.UpdateSkill();

        //��ų ������̸� ����
        if (_coWait != null)
        {
            return;
        }
    }
    #endregion

    #region Battle
    public override bool IsEnemy(BaseObject target)
    {
        if (base.IsEnemy(target) == false)
            return false;

        return target.ObjectType == EGameObjectType.Monster;
    }

    public override void HandleSkillPacket(S_Skill packet)
    {
        base.HandleSkillPacket(packet);
    }
    #endregion


}
