using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Game
{
    public abstract class Skill
    {
        public Creature Owner { get; protected set; }
        public int TemplatedId {get; protected set; }

        protected SkillData _skillData;

        // 쿨타임 관리
        public long NextUseTick { get; protected set; } = 0;

        public Skill(int templatedId, Creature owner)
        {
            TemplatedId = templatedId;
            Owner = owner;

            DataManager.SkillDict.TryGetValue(TemplatedId, out SkillData skillData);
            _skillData = skillData;
        }

        public abstract bool CanUseSkill(SkillContext skillContext);
        public abstract void UseSkill(SkillContext skillContext);

        #region 쿨타임 관리
        public long GetRemainingCooltimeInTicks()
        {
            return Math.Max(0, (NextUseTick - Utils.TickCount));
        }

        public float GetRemainingCooltimeInSeconds()
        {
            return GetRemainingCooltimeInTicks() / 1000.0f;
        }

        public void UpdateCooltime()
        {
            NextUseTick = Utils.TickCount + (long)(1000 * _skillData.Cooldown);
        }
        #endregion

        #region 스킬 사용
        public bool CheckCooltimeAndState()
        {
            if (CheckCooltime() == false)
                return false;
            if(Owner.Room == null)
                return false;
            if (Owner.State == EObjectState.Dead)
                return false;
            if (Owner.IsStunned)
                return false;

            return true;
        }

        public bool CheckCooltime()
        {
            return GetRemainingCooltimeInTicks() == 0;
        }

        public bool CheckTargetAndRange(SkillContext skillContext)
        {
            if (skillContext == null)
                return false;
            Creature target = GetUseSkillTarget(Owner, _skillData, skillContext);
            if (target == null)
                return false;
            int dist = Owner.GetDistance(target);
            if(dist > _skillData.SkillRange)
                return false;

            return true;
        }

        public static bool IsValidUseSkillTargetType(Creature owner, Creature target, EUseSkillTargetType targetType)
        {
            switch(targetType)
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
            switch(targetType)
            {
                case ETargetFriendType.Friend:
                    return owner.IsFriend(target);
                case ETargetFriendType.Enemy:
                    return owner.IsEnemy(target);
            }

            return true;
        }

        public static Creature GetUseSkillTarget(Creature owner, SkillData skillData, SkillContext skillContext)
        {
            if (owner.Room == null)
                return null;
            if (skillContext == null)
                return null;

            Creature target = owner.Room.GetCreatureById(skillContext.TargetId);

            if (IsValidUseSkillTargetType(owner, target, skillData.UseSkillTargetType) == false)
                return null;

            if (IsValidTargetFriendType(owner, target, skillData.TargetFriendType) == false)
                return null;

            return target;
        }

        public static List<Creature> GatherSkillEffectTargets(Creature owner, SkillData skillData, Creature target)
        {
            List<Creature> targets = new List<Creature>();

            if (owner.Room == null)
                return targets;

            bool isSingleTarget = (skillData.GatherTargetRange == 0);

            if(isSingleTarget)
            {
                if(IsValidTargetFriendType(owner, target, skillData.TargetFriendType))
                    targets.Add(target);
            }
            else
            {
                targets = owner.Room.FindAdjancentCreatures(owner.CellPos, (c) =>
                {
                    if (IsValidTargetFriendType(owner, c, skillData.TargetFriendType) == false)
                        return false;

                    int dist = owner.GetDistance(c);
                    if (dist > skillData.GatherTargetRange)
                        return false;

                    return true;
                });
            }

            return targets;
        }

        protected static void AddEffect(Creature target, Creature caster, EffectData effectData)
        {
            
        }

        protected void BroadcastSkill(Creature target)
        {

        }
        #endregion
    }
}
