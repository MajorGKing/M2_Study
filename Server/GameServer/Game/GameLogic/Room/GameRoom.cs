using GameServer.Game;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public partial class GameRoom : JobSerializer
    {
        public const int VisionCells = 10;
        public int GameRoomId { get; set; }
        public int TemplateId { get; set; }

        Dictionary<int, Hero> _heroes = new Dictionary<int, Hero>();
        Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();

        public MapComponent Map { get; private set; } = new MapComponent();
        public SpawningPoolComponent SpawningPool { get; private set; } = new SpawningPoolComponent();

        // GameRoom이라는 거대한 공간을 Zone이라는 단위로 균일하게 세분화
        public Zone[,] Zones { get; private set; } // 인근 오브젝트를 빠르게 찾기 위한 일종의 캐시이다
        public int ZoneCells { get; private set; } // 하나의 존을 구성하는 셀 개수

        Random _rand = new Random();

        public void Init(int mapTemplateId, int zoneCells)
        {
            TemplateId = mapTemplateId;

            Map.LoadMap();

            // Zone
            ZoneCells = zoneCells; // 10
                                   // 1~10 칸 = 1존
                                   // 11~20칸 = 2존
                                   // 21~30칸 = 3존

            int countX = (Map.SizeX + zoneCells - 1) / zoneCells;
            int countY = (Map.SizeY + zoneCells - 1) / zoneCells;
            Zones = new Zone[countX, countY];
            for (int x = 0; x < countX; x++)
            {
                for (int y = 0; y < countY; y++)
                {
                    Zones[x, y] = new Zone(x, y);
                }
            }

            SpawningPool.Init(this);
            Push(SpawningPool.Update);
        }

        // 누군가 주기적으로 호출해줘야 한다
        public void Update()
        {
            //Console.WriteLine($"TimerCount : {TimerCount}");
            //Console.WriteLine($"JobCount : {JobCount}");
            Flush();
        }

        public void EnterGame(BaseObject obj, bool respawn = false, Vector2Int? cellPos = null)
        {
            if (obj == null)
                return;

            EGameObjectType type = ObjectManager.GetObjectTypeFromId(obj.ObjectId);

            if (type == EGameObjectType.Hero)
            {
                Hero hero = (Hero)obj;

                // 1. 오브젝트 추가 및 방 설정
                _heroes.Add(obj.ObjectId, hero);
                hero.Room = this;

                // 2. 아직 점유되지 않는 알맞는 좌표를 찾아주기.
                FindAndSetCellPos(obj, cellPos);

                // 3. 맵에 실제 적용하고 충돌 그리드 갱신한다.
                Map.ApplyMove(hero, hero.CellPos);

                // 4. 존(캐싱)에도 해당 정보 추가.
                GetZone(hero.CellPos).Heroes.Add(hero);

                // 5. 틱 시작.
                hero.State = EObjectState.Idle;
                hero.Update();

                // 6. 입장한 사람한테 패킷 보내기.
                {
                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.MyHeroInfo = hero.MyHeroInfo;
                    enterPacket.Respawn = respawn;

                    hero.Session?.Send(enterPacket);
                }

                // 7. 다른 사람들한테 입장 알려주기.
                S_Spawn spawnPacket = new S_Spawn();
                spawnPacket.Heroes.Add(hero.HeroInfo);
                Broadcast(obj.CellPos, spawnPacket);


                // 8. AOI 틱 시작.
                hero.Vision?.Update();
            }
            else if(type == EGameObjectType.Monster)
            {
                Console.WriteLine("Monster spawned");
                Monster monster = (Monster)obj;

                // 1. 오브젝트 추가 및 방 설정.
                _monsters.Add(obj.ObjectId, monster);
                monster.Room = this;

                // 2. 아직 점유되지 않는 알맞는 좌표를 찾아주기.
                FindAndSetCellPos(obj, cellPos);

                // 3. 맵에 실제 적용하고 충돌 그리드 갱신한다.
                Map.ApplyMove(monster, monster.CellPos);

                // 4. 존(캐싱)에도 해당 정보 추가.
                GetZone(monster.CellPos).Monsters.Add(monster);

                // 5. 틱 시작.
                monster.State = EObjectState.Idle;
                monster.Update();

                // 6. 다른 사람들한테 입장 알려주기.
                S_Spawn spawnPacket = new S_Spawn();
                spawnPacket.Creatures.Add(monster.CreatureInfo);
                Broadcast(obj.CellPos, spawnPacket);
            }
        }

        public void LeaveGame(int objectId, bool kick = false)
        {
            EGameObjectType type = ObjectManager.GetObjectTypeFromId(objectId);

            Vector2Int cellPos;

            if (type == EGameObjectType.Hero)
            {
                if (_heroes.Remove(objectId, out Hero hero) == false)
                    return;

                // 1. 맵에 실제 적용하고 충돌 그리드 갱신한다.
                Map.ApplyLeave(hero);

                // 2. 오브젝트 제거 및 방 제거.
                _heroes.Remove(objectId);
                hero.Room = null;

                // 3. 퇴장한 사람한테 패킷 보내기.
                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    hero.Session?.Send(leavePacket);
                }

                // 4. 다른 사람들한테 퇴장 알려주기.
                S_Despawn despawnPacket = new S_Despawn();
                despawnPacket.ObjectIds.Add(objectId);
                Broadcast(hero.CellPos, despawnPacket);

                // 5. AOI 정리.
                hero.Vision?.Clear();

                // 6. DB에 좌표 등 정보 저장.
                DBManager.SaveHeroDbNoti(hero);

                //if (kick)
                //{
                //    // 로비로 강퇴
                //    //S_Kick kickPacket = new S_Kick();
                //    //player.Session?.Send(kickPacket);
                //}
            }
            else if(type == EGameObjectType.Monster)
            {
                Console.WriteLine("Monster leaved");
                if (_monsters.TryGetValue(objectId, out Monster monster) == false)
                    return;

                // 1. 맵에 실제 적용하고 충돌 그리드 갱신한다.
                Map.ApplyLeave(monster);

                // 2. 오브젝트 제거 및 방 제거.
                _monsters.Remove(objectId);
                monster.Room = null;

                // 3. 다른 사람들한테 퇴장 알려주기.
                S_Despawn despawnPacket = new S_Despawn();
                despawnPacket.ObjectIds.Add(objectId);
                Broadcast(monster.CellPos, despawnPacket);
            }
            else
            {
                return;
            }
        }

        public Zone GetZone(Vector2Int cellPos)
        {
            int x = (cellPos.x - Map.MinX) / ZoneCells;
            int y = (Map.MaxY - cellPos.y) / ZoneCells;

            return GetZone(x, y);
        }

        public Zone GetZone(int indexX, int indexY)
        {
            if (indexX < 0 || indexX >= Zones.GetLength(0))
                return null;
            if (indexY < 0 || indexY >= Zones.GetLength(1))
                return null;

            return Zones[indexX, indexY];
        }

        public void Broadcast(Vector2Int pos, IMessage packet)
        {
            List<Zone> zones = GetAdjacentZones(pos);
            if (zones.Count == 0)
                return;

            byte[] packetBuffer = ClientSession.MakeSendBuffer(packet);

            foreach (Hero p in zones.SelectMany(z => z.Heroes))
            {
                int dx = p.CellPos.x - pos.x;
                int dy = p.CellPos.y - pos.y;
                if (Math.Abs(dx) > GameRoom.VisionCells)
                    continue;
                if (Math.Abs(dy) > GameRoom.VisionCells)
                    continue;

                p.Session?.Send(packetBuffer);
            }
        }

        public List<Zone> GetAdjacentZones(Vector2Int cellPos, int cells = GameRoom.VisionCells)
        {
            HashSet<Zone> zones = new HashSet<Zone>();

            int maxY = cellPos.y + cells;
            int minY = cellPos.y - cells;
            int maxX = cellPos.x + cells;
            int minX = cellPos.x - cells;

            // 좌측 상단
            Vector2Int leftTop = new Vector2Int(minX, maxY);
            int minIndexY = (Map.MaxY - leftTop.y) / ZoneCells;
            int minIndexX = (leftTop.x - Map.MinX) / ZoneCells;

            // 우측 하단
            Vector2Int rightBot = new Vector2Int(maxX, minY);
            int maxIndexY = (Map.MaxY - rightBot.y) / ZoneCells;
            int maxIndexX = (rightBot.x - Map.MinX) / ZoneCells;

            for (int x = minIndexX; x <= maxIndexX; x++)
            {
                for (int y = minIndexY; y <= maxIndexY; y++)
                {
                    Zone zone = GetZone(x, y);
                    if (zone == null)
                        continue;

                    zones.Add(zone);
                }
            }

            return zones.ToList();
        }

        // TODO : 임시 버전
        public Vector2Int GetRandomSpawnPos(BaseObject obj, bool checkObjects = true)
        {
            Vector2Int randomPos;

            int delta = 10;
            const int tryCount = 100;

            while (true)
            {
                for (int i = 0; i < tryCount; i++)
                {
                    randomPos.x = _rand.Next(-delta, delta) + obj.CellPos.x;
                    randomPos.y = _rand.Next(-delta, delta) + obj.CellPos.x;

                    if (Map.CanGo(obj, randomPos, checkObjects: true))
                        return randomPos;
                }

                delta *= 2;
            }
        }

        public List<T> FindAdjacents<T>(Vector2Int pos, Func<T, bool> condition = null, int cells = GameRoom.VisionCells) where T : BaseObject
        {
            List<T> objs = new List<T>();
            List<Zone> zones = GetAdjacentZones(pos, cells);

            if (typeof(T) == typeof(Hero))
            {
                foreach (Hero p in zones.SelectMany(z => z.Heroes))
                {
                    int dx = p.CellPos.x - pos.x;
                    int dy = p.CellPos.y - pos.y;
                    if (Math.Abs(dx) > GameRoom.VisionCells)
                        continue;
                    if (Math.Abs(dy) > GameRoom.VisionCells)
                        continue;
                    if (condition == null || condition.Invoke(p as T) == false)
                        continue;

                    objs.Add(p as T);
                }
            }
            else if (typeof(T) == typeof(Monster))
            {
                foreach(Monster m in zones.SelectMany(z => z.Monsters))
                {
                    int dx = m.CellPos.x - pos.x;
                    int dy = m.CellPos.y - pos.y;
                    if (Math.Abs(dx) > GameRoom.VisionCells)
                        continue;
                    if (Math.Abs(dy) > GameRoom.VisionCells)
                        continue;
                    if (condition == null || condition.Invoke(m as T) == false)
                        continue;

                    objs.Add(m as T);
                }
            }

            return objs;
        }

        private void FindAndSetCellPos(BaseObject obj, Vector2Int? pos = null)
        {
            if(pos.HasValue && Map.CanGo(obj, pos.Value, checkObjects:true))
                obj.CellPos = pos.Value;
            else
                obj.CellPos = GetRandomSpawnPos(obj, checkObjects:true);
        }
    }
}
