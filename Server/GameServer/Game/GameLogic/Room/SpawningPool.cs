using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
            foreach (RespawnData respawnData in spawningPoolData.RespawnDatas)
            {
                for (int i = 0; i < respawnData.Count; i++)
                {
                    Monster monster = ObjectManager.Instance.Spawn<Monster>(respawnData.MonsterDataId);
                    monster.SpawnRange = respawnData.SpawnRange;
                    _gameobjects.Add(monster.ObjectId, monster);

                    // 몬스터 생성.
                    Vector2Int pivotPos = new Vector2Int(respawnData.PivotPosX, respawnData.PivotPosY);
                    Vector2Int pos = GetRandomSpawnPos(monster, respawnData.SpawnRange, pivotPos);
                    Owner.Push(() => Owner.EnterGame(monster, pos, false));
                }
            }

            // NPC
            foreach (NpcData npcData in Owner.RoomData.Npcs)
            {
                Npc npc = null;

                switch (npcData.NpcType)
                {
                    case ENpcType.Common:
                        npc = ObjectManager.Instance.Spawn<Npc>(npcData.TemplateId);
                        break;
                    case ENpcType.Portal:
                        npc = ObjectManager.Instance.Spawn<Npc>(npcData.TemplateId);
                        break;
                    case ENpcType.Shop:
                        break;
                }

                if (npc == null)
                    continue;

                _gameobjects.Add(npc.ObjectId, npc);

                // Npc 생성
                Vector2Int spawnPos = new Vector2Int(npcData.SpawnPosX, npcData.SpawnPosY);
                Owner.Push(() => Owner.EnterGame(npc, spawnPos, false));
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

                RespawnData respawnData = _spawningPoolData.RespawnDatas.Find(x => x.MonsterDataId == monster.TemplateId);
                if (respawnData == null)
                {
                    Console.WriteLine($"invalid respawn. monster templateId not found. TemplateId [{monster.TemplateId}]");
                    return;
                }

                // 1. 방에서 제거.
                Owner.LeaveGame(go.ObjectId, ELeaveType.None);

                // 2. 리스폰 예약.
                Owner.PushAfter(respawnData.RespawnTime * 1000, () =>
                {
                    // 몬스터 리셋.
                    monster.Reset();
                    // 입장
                    Vector2Int pivotPos = new Vector2Int(respawnData.PivotPosX, respawnData.PivotPosY);
                    Vector2Int pos = GetRandomSpawnPos(monster, respawnData.SpawnRange, pivotPos);
                    Owner.EnterGame(monster, pos, true);
                });

                return;
            }
        }

        // TODO : 임시 버전
        public Vector2Int GetRandomSpawnPos(BaseObject obj, int delta, Vector2Int pivot, bool checkObjects = true)
        {
            Vector2Int randomPos;

            Vector2Int cellPos;

            cellPos = pivot;

            randomPos.x = _rand.Next(-delta, delta) + cellPos.x;
            randomPos.y = _rand.Next(-delta, delta) + cellPos.y;

            return randomPos;
        }
    }
}
