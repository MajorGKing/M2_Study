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
    internal class ProjectileSkill : Skill
    {
        public ProjectileSkill(int templatedId, Creature owner) : base(templatedId, owner)
        {
        }

        public override bool CanUseSkill(SkillContext skillContext)
        {
            if (CheckCooltimeAndState() == false)
                return false;
            if (CheckTargetAndRange(skillContext) == false)
                return false;
            if (_skillData.Projectile == null)
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
            if (target == null)
                return;
            Projectile projectile = ObjectManager.Instance.Spawn<Projectile>(_skillData.Projectile.TemplateId);
            if (projectile == null)
                return;

            projectile.Init(_skillData, target);
            projectile.Owner = Owner;
            projectile.PosInfo.State = EObjectState.Move;
            projectile.PosInfo.MergeFrom(Owner.PosInfo);
        }
    }
}
