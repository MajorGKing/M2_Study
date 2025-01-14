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

        public override bool CanUseSkill(int targetId)
        {
            if (CheckCooltimeAndState() == false)
                return false;
            if (CheckTargetAndRange(targetId) == false)
                return false;

            return true;
        }

        public override void UseSkill(int targetId)
        {
            if (CanUseSkill(targetId) == false)
                return;
            GameRoom room = Owner.Room;
            if (room == null)
                return;

            Creature target = GetUseSkillTarget(Owner, _skillData, targetId);

            // 이펙트(효과 및 데미지) 적용
            List<Creature> targets = GatherSkillEffectTargets(Owner, _skillData, target);
            foreach(Creature t in targets)
            {
                Console.WriteLine($"{Owner} attack {t.ObjectId} by normal {_skillData.EffectData.Name}!");
                room.PushAfter((int)(_skillData.DelayTime * 1000), AddEffect, t, Owner, _skillData.EffectData);
            }

            BroadcastSkill(target);
        }
    }
}
