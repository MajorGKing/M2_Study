using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Game
{
    // TODO : 어느 위치에, 어느 범위에, 어떤 오브젝트를, 어떤 주기로?
    public class SpawningPoolComponent
    {
        public GameRoom Owner { get; private set; }

        SpawningPoolData _spawningPoolData = null;
        Random _rand = new Random();

        //Dictionary<int/*templateId*/, int/*count*/> _monsters = new Dictionary<int, int>();
        Dictionary<int, BaseObject> _gameobjects = new Dictionary<int, BaseObject>();

        public SpawningPoolComponent()
        {

        }

        public void Init(GameRoom owner)
        {
            if (DataManager.SpawningPoolDict.TryGetValue(owner.TemplateId, out SpawningPoolData spawningPoolData) == false)
                return;

            Owner = owner;
            _spawningPoolData = spawningPoolData;

            // Monster
            foreach (RespawnInfo respawnInfo in spawningPoolData.monsters)
            {
                for (int i = 0; i < respawnInfo.Count; i++)
                {
                    Monster monster = ObjectManager.Instance.Spawn<Monster>(respawnInfo.TemplateId);
                    _gameobjects.Add(monster.ObjectId, monster);

                    // 몬스터 생성.
                    Owner.Push(() => Owner.EnterGame(monster, true));
                }
            }
        }

        public T Find<T>(int objectId) where T : BaseObject, new()
        {
            if (_gameobjects.TryGetValue(objectId, out BaseObject go))
                return go as T;

            return null;
        }

        public void Update()
        {
            if (Owner == null)
                return;
            if (_spawningPoolData == null)
                return;

            Owner.PushAfter(1000, Update);
        }

        public void Respawn(BaseObject go)
        {
            if (go is Monster)
            {
                Monster monster = go as Monster;

                RespawnInfo respawnInfo = _spawningPoolData.monsters.Find(x => x.TemplateId == monster.TemplateId);
                if (respawnInfo == null)
                {
                    Console.WriteLine($"invalid respawn. monster templateId not found. TemplateId [{monster.TemplateId}]");
                    return;
                }

                // 1. 몬스터 리셋.
                monster.Reset();

                // 2. 방에서 제거.
                Owner.LeaveGame(go.ObjectId);

                // 3. 리스폰 예약.
                Owner.PushAfter(respawnInfo.respawnTime * 1000, () =>
                {
                    Owner.EnterGame(monster, true);
                });

                return;
            }
        }
    }
}
