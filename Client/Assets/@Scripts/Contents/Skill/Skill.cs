using Google.Protobuf.Protocol;
using Scripts.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill
{
    public MyHero Owner { get; protected set; }
    public int TemplatedId { get; protected set; }

    public SkillData SkillData { get; private set; }

    // ÄðÅ¸ÀÓ °ü¸®
    public long NextUseTick { get; protected set; } = 0;

    public Skill(int templatedId, MyHero owner)
    {
        TemplatedId = templatedId;
        Owner = owner;

        Managers.Data.SkillDic.TryGetValue(TemplatedId, out SkillData skillData);
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
        if (dist > SkillData.SkillRange)
            return ECanUseSkillFailReason.SkillRange;

        if (Owner.Mp < SkillData.Cost)
            return ECanUseSkillFailReason.SkillCost;

        return ECanUseSkillFailReason.None;
    }
}
