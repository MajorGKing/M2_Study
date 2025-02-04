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

    private int _baseAttackIndex = 0;

    private string[] _baseAttackAnimName = new[]
    {
        "attack_a", "attack_b", "attack_c"
    };
    private string[] _baseAttackParticleName = new[]
    {
        "_Attack_a_Trail","_Attack_b_Trail","_Attack_c_Trail",
    };

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

    #region ���� , �ִϸ��̼� ��
    protected override void ExecuteSkillAction(int skillTemplateId)
    {
        if (Managers.Data.SkillDict.TryGetValue(skillTemplateId, out SkillData skillData) == false)
            return;

        if (Managers.Skill.IsMainSkill(skillTemplateId))
        {
            ExecuteMainSkillAction(skillData);
        }
        else
        {
            ExecuteSecondarySkillAction(skillData);
        }
    }

    private void ExecuteMainSkillAction(SkillData skillData)
    {
        PlayAnimation(0, _baseAttackAnimName[_baseAttackIndex], false);
        AddAnimation(0, AnimName.IDLE, true, 0);

        string particleName = $"{HeroInfo.ClassType}{_baseAttackParticleName[_baseAttackIndex]}";
        Managers.Object.SpawnParticle(particleName, LookLeft, transform, false);

        UpdateBaseAttackIndex();
    }

    private void ExecuteSecondarySkillAction(SkillData skillData)
    {
        PlayAnimation(0, skillData.AnimName, false);
        AddAnimation(0, AnimName.IDLE, true, 0);

        if (!string.IsNullOrEmpty(skillData.PrefabName))
        {
            Managers.Object.SpawnParticle(skillData.PrefabName, LookLeft, transform, false);
        }
    }

    private void UpdateBaseAttackIndex()
    {
        _baseAttackIndex++;
        if (_baseAttackIndex > 2)
            _baseAttackIndex = 0;
    }
    #endregion

}
