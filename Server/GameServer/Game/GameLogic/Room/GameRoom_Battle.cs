using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game;

namespace GameServer
{
    public partial class GameRoom : JobSerializer
    {
        public void HandleMove(Hero hero, C_Move movePacket)
        {
            if (hero == null)
                return;
            if (hero.State == EObjectState.Dead)
                return;

            PositionInfo movePosInfo = movePacket.PosInfo;
            ObjectInfo info = hero.ObjectInfo;

            Console.WriteLine(movePosInfo.State);

            // TODO : 거리 검증 등

            if (Map.CanGo(hero, new Vector2Int(movePosInfo.PosX, movePosInfo.PosY)) == false)
                return;

            info.PosInfo.State = movePosInfo.State;
            info.PosInfo.MoveDir = movePosInfo.MoveDir;
            Map.ApplyMove(hero, new Vector2Int(movePosInfo.PosX, movePosInfo.PosY));

            Console.WriteLine($"POSX : {movePosInfo.PosX} POSY : {movePosInfo.PosY}");

            hero.BroadcastMove();
        }

        public void UseSkill(Creature owner, int templateId, SkillContext skillContext)
        {
            if (owner == null)
                return;

            owner.SkillComp.UseSkill(templateId, skillContext);
        }

        public void OnDead(BaseObject gameObject, BaseObject attacker)
        {
            if (gameObject.ObjectType == EGameObjectType.Projectile)
                return;
            if (gameObject.State == EObjectState.Dead)
                return;

            gameObject.State = EObjectState.Dead;

            if(gameObject.ObjectType == EGameObjectType.Hero)
            {
                LeaveGame(gameObject.ObjectId);

                // TODO : 마을에서 리스폰
                Hero hero = gameObject as Hero;
                hero.Reset();

                PushAfter(3000, () =>
                {
                    EnterGame(hero);
                });

                return;
            }
            else if(gameObject.ObjectType == EGameObjectType.Monster)
            {
                gameObject.Room.SpawningPool.Respawn(gameObject);
            }
        }
    }
}
