using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using Data;
using UnityEngine;

public abstract class Skill
{
    public MyHero Owner { get; protected set; }
    public int TemplateId { get; protected set; }

    public SkillData SkillData { get; private set; }

    // 쿨타임 관리
    public long NextUseTick { get; protected set; } = 0;

    public Skill(int templateId, MyHero owner)
    {
        TemplateId = templateId;
        Owner = owner;

        Managers.Data.SkillDict.TryGetValue(TemplateId, out SkillData skillData);
        SkillData = skillData;
    }

    public virtual ECanUseSkillFailReason CanUseSkill(Creature target)
    {
        if (CheckCooltime() == false)
            return ECanUseSkillFailReason.Cooltime;

        if (Owner.IsStunned)
            return ECanUseSkillFailReason.InvalidOwnerState;

        if (IsValidSkillTarget(Owner, SkillData, target) == false)
            return ECanUseSkillFailReason.InvalidTarget;

        int dist = Owner.GetDistance(target);
        if (dist > GetSkillRange(target))
            return ECanUseSkillFailReason.SkillRange;

        if (Owner.Mp < SkillData.Cost)
            return ECanUseSkillFailReason.SkillCost;

        return ECanUseSkillFailReason.None;
    }

    public abstract void UseSkill(Creature target);

    public int GetSkillRange(Creature target)
    {
        return SkillData.SkillRange + target.ExtraCells + Owner.ExtraCells;
    }

    #region 쿨타임 관리
    public long GetRemainingCooltimeInTicks()
    {
        return Math.Max(0, (NextUseTick - Utils.TickCount));
    }

    public float GetRemainingCooltimeInSeconds()
    {
        return GetRemainingCooltimeInTicks() / 1000.0f;
    }

    public float GetRemainingCoolTimeRatio()
    {
        return GetRemainingCooltimeInSeconds() / SkillData.Cooltime;
    }

    public void UpdateCooltime()
    {
        NextUseTick = Utils.TickCount + (long)(1000 * SkillData.Cooltime);
    }

    public void SetRemainingCooltime(long remainTicks)
    {
        NextUseTick = Utils.TickCount + remainTicks;
    }
    #endregion

    #region 스킬 사용
    public List<Creature> GatherSkillEffectTargets(Creature owner, SkillData skillData, Creature target)
    {
        List<Creature> targets = new List<Creature>();

        if (skillData.IsSingleTarget)
        {
            if (IsValidTargetFriendType(owner, target, skillData.TargetFriendType))
                targets.Add(target);
        }
        else
        {
            bool isSelfTarget = skillData.UseSkillTargetType == EUseSkillTargetType.Self;
            Vector3Int pivot = isSelfTarget ? owner.CellPos : target.CellPos;

            targets = Managers.Object.FindCreatures(pivot, (c) =>
            {
                if (IsValidTargetFriendType(owner, c, skillData.TargetFriendType) == false)
                    return false;

                int dist = isSelfTarget ? owner.GetDistance(c) : target.GetDistance(c);

                if (dist > skillData.GatherTargetRange)
                    return false;

                return true;
            });
        }

        return targets;
    }
    public bool CheckCooltime()
    {
        return GetRemainingCooltimeInTicks() == 0;
    }

    public static bool IsValidUseSkillTargetType(Creature owner, Creature target, EUseSkillTargetType targetType)
    {
        switch (targetType)
        {
            case EUseSkillTargetType.Self:
                return owner == target;
            case EUseSkillTargetType.Other:
                return owner != target;
        }

        return true;
    }

    public static bool IsValidTargetFriendType(Creature owner, Creature target, ETargetFriendType targetType)
    {
        switch (targetType)
        {
            case ETargetFriendType.Friend:
                return owner.IsFriend(target);
            case ETargetFriendType.Enemy:
                return owner.IsEnemy(target);
        }

        return true;
    }

    public static bool IsValidSkillTarget(MyHero owner, SkillData skillData, Creature target)
    {
        if (skillData.UseSkillTargetType == EUseSkillTargetType.Self)
        {
            //SelfCenter 범위기는 target검사 X
            if (skillData.IsSingleTarget == false)
                return true;

            if (target != owner)
                return false;
        }
        else
        {
            if (target == null)
                return false;
        }

        if (IsValidUseSkillTargetType(owner, target, skillData.UseSkillTargetType) == false)
            return false;

        if (IsValidTargetFriendType(owner, target, skillData.TargetFriendType) == false)
            return false;

        return true;
    }

    public void ReqUseSkill()
    {
        MyHero hero = Owner;
        if (hero == null)
            return;

        hero.ReqUseSkill(TemplateId);
    }
    #endregion
}