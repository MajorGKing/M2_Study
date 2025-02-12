using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class BaseObject
    {
        public EGameObjectType ObjectType { get; protected set; } = EGameObjectType.None;
        public int ObjectId
        {
            get { return ObjectInfo.ObjectId; }
            set { ObjectInfo.ObjectId = value; }
        }
        public int TemplateId { get; protected set; }

        public int ExtraCells { get; protected set; } = 0;

        public GameRoom Room { get; set; }

        public ObjectInfo ObjectInfo { get; set; } = new ObjectInfo();
        public PositionInfo PosInfo { get; private set; } = new PositionInfo();

        public EObjectState State
        {
            get { return PosInfo.State; }
            set { PosInfo.State = value; }
        }

        public Vector2Int CellPos
        {
            get
            {
                return new Vector2Int(PosInfo.PosX, PosInfo.PosY);
            }

            set
            {
                PosInfo.PosX = value.x;
                PosInfo.PosY = value.y;
            }
        }

        public EMoveDir MoveDir
        {
            get
            {
                return PosInfo.MoveDir;
            }
            set
            {
                PosInfo.MoveDir = value;
            }
        }

        public BaseObject()
        {
            ObjectInfo.PosInfo = PosInfo;
        }

        public virtual void Update()
        {

        }

        public void BroadcastMove()
        {
            // 다른 플레이어한테도 알려준다
            S_Move movePacket = new S_Move();
            movePacket.ObjectId = ObjectId;
            movePacket.PosInfo = PosInfo;
            Room?.Broadcast(CellPos, movePacket);
        }

        public virtual float OnDamaged(BaseObject attacker, float damage)
        {
            return 0;
        }

        public virtual void OnDead(BaseObject attacker)
        {
            if(Room == null)
                return;

            S_Die diePacket = new S_Die();
            diePacket.ObjectId = ObjectId;
            diePacket.AttackerId = attacker.ObjectId;

            Room.Broadcast(CellPos, diePacket);
            Room.OnDead(this, attacker);
        }

        public Vector2Int GetFrontCellPos()
        {
            return GetFrontCellPos(PosInfo.MoveDir);
        }

        public Vector2Int GetFrontCellPos(EMoveDir dir, int cells = 1)
        {
            Vector2Int cellPos = CellPos;

            switch (dir)
            {
                case EMoveDir.Up:
                    cellPos += new Vector2Int(1, 1) * cells;
                    break;
                case EMoveDir.Down:
                    cellPos += new Vector2Int(-1, -1) * cells; 
                    break;
                case EMoveDir.Left:
                    cellPos += new Vector2Int(-1, 1) * cells; 
                    break;
                case EMoveDir.Right:
                    cellPos += new Vector2Int(1, -1) * cells;
                    break;
                case EMoveDir.UpLeft:
                    cellPos += new Vector2Int(0, 1) * cells;
                    break;
                case EMoveDir.UpRight:
                    cellPos += new Vector2Int(1, 0) * cells;
                    break;
                case EMoveDir.DownLeft:
                    cellPos += new Vector2Int(-1, 0) * cells;
                    break;
                case EMoveDir.DownRight:
                    cellPos += new Vector2Int(0, -1) * cells;
                    break;
            }

            return cellPos;
        }

        public List<Vector2Int> GetFrontCellPosList(EMoveDir dir, int range)
        {
            List<Vector2Int> ret = new List<Vector2Int>();

            Vector2Int cellPos = CellPos;
            int extraCells = ExtraCells;

            switch (dir)
            {
                case EMoveDir.Up:
                    {
                        cellPos += Vector2Int.up * (1 + extraCells);
                        for (int dx = -extraCells; dx <= extraCells; dx++)
                        {
                            ret.Add(new Vector2Int(cellPos.x + dx, cellPos.y));
                        }
                    }
                    break;
                case EMoveDir.Down:
                    {
                        cellPos += Vector2Int.down * (1 + extraCells);
                        for (int dx = -extraCells; dx <= extraCells; dx++)
                        {
                            ret.Add(new Vector2Int(cellPos.x + dx, cellPos.y));
                        }
                    }
                    break;
                case EMoveDir.Left:
                    {
                        cellPos += Vector2Int.left * (1 + extraCells);
                        for (int dy = -extraCells; dy <= extraCells; dy++)
                            ret.Add(new Vector2Int(cellPos.x, cellPos.y + dy));
                    }
                    break;
                case EMoveDir.Right:
                    {
                        cellPos += Vector2Int.right * (1 + extraCells);
                        for (int dy = -extraCells; dy <= extraCells; dy++)
                            ret.Add(new Vector2Int(cellPos.x, cellPos.y + dy));
                    }
                    break;
            }

            return ret;
        }

        public float GetActualDistance()
        {
            switch (MoveDir)
            {
                case EMoveDir.None:
                    return 1f;
                case EMoveDir.Up:
                case EMoveDir.Down:
                    return Define.TILE_HEIGHT;
                case EMoveDir.Left:
                case EMoveDir.Right:
                    return Define.TILE_WIDTH;
                default:
                    return Define.DIAGONAL_DISTANCE;
            }
        }

        public int GetDistance(BaseObject target)
        {
            return GetDistance(target.CellPos);
        }

        public int GetDistance(Vector2Int pos)
        {
            return Utils.GetDistance(pos, CellPos);
        }

        public Vector2Int GetClosestBodyCellPointToTarget(BaseObject target)
        {
            Vector2Int cellPoint = Vector2Int.zero;
            int minDist = int.MaxValue;
            for (int dx = -target.ExtraCells; dx <= target.ExtraCells; dx++)
            {
                for(int dy = -target.ExtraCells; dy <= target.ExtraCells; dy++)
                {
                    Vector2Int checkPos = new Vector2Int(target.CellPos.x + dx, target.CellPos.y + dy);
                    int dist = GetDistance(checkPos);
                    if(dist < minDist)
                    {
                        minDist = dist;
                        cellPoint = checkPos;
                    }
                }
            }

            return cellPoint;
        }

        public virtual BaseObject GetOwner()
        {
            return this;
        }
    }
}
