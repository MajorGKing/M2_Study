using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GameServer.Game
{
    public class NormalSkill : Skill
    {
        public NormalSkill(int templatedId, Creature owner) : base(templatedId, owner)
        {
        }

        public override bool CanUseSkill(SkillContext skillContext)
        {
            if (CheckCooltimeAndState() == false)
                return false;
            if (CheckTargetAndRange(skillContext) == false)
                return false;

            return true;
        }

        public override void UseSkill(SkillContext skillContext)
        {
            if (CanUseSkill(skillContext) == false)
                return;
            GameRoom room = Owner.Room;
            if (room == null)
                return;

            Creature target = GetUseSkillTarget(Owner, _skillData, skillContext);

            // 이펙트(효과 및 데미지) 적용
            List<Creature> targets = GatherSkillEffectTargets(Owner, _skillData, target);
            foreach(Creature t in targets)
            {
                room.PushAfter((int)(_skillData.DelayTime * 1000), AddEffect, t, Owner, _skillData.EffectData);
            }

            BroadcastSkill(target);
        }
    }
}
