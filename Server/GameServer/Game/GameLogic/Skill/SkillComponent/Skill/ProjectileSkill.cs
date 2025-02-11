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

        public override bool CanUseSkill(int targetId)
        {
            if (CheckCooltimeAndState() == false)
                return false;
            if (CheckTargetAndRange(targetId) == false)
                return false;
            if (SkillData.ProjectileId == null)
                return false;
            if (Owner.Mp < SkillData.Cost)
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
            Creature target = GetUseSkillTarget(Owner, SkillData, targetId);
            if (target == null)
                return;
            Projectile projectile = ObjectManager.Instance.Spawn<Projectile>(SkillData.ProjectileId);
            if (projectile == null)
                return;

            if (SkillData.Cost > 0)
                Owner.Heal(EStatType.Mp, -SkillData.Cost);

            projectile.Init(SkillData, target);
            
            projectile.Owner = Owner;
            projectile.ProjectileInfo.OwnerId = Owner.ObjectId;

            projectile.PosInfo.State = EObjectState.Move;
            projectile.PosInfo.MergeFrom(Owner.PosInfo);

            // 애니메이션 이벤트타임에 맞게 프로젝타일 생성
            Vector2Int spawnPos = new Vector2Int(Owner.PosInfo.PosX, Owner.PosInfo.PosY);
            room.PushAfter((int)(SkillData.DelayTime * 1000), () =>
            {
                room.EnterGame(projectile, spawnPos, false);
            });

            BroadcastSkill(target);
        }
    }
}
