using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using static Define;

public class MapManager
{
    public GameObject Map { get; private set; }
    public string MapName { get; private set; }
    public Grid CellGrid { get; private set; }
    public Dictionary<Vector3Int, BaseObject> _cells = new Dictionary<Vector3Int, BaseObject>();

    public int MinX { get; private set; }
    public int MaxX { get; private set; }
    public int MinY { get; private set; }
    public int MaxY { get; private set; }

    private ECellCollisionType[,] _collision;

    public Vector3Int World2Cell(Vector3 worldPos)
    {
        return CellGrid.WorldToCell(worldPos);
    }

    public Vector3 Cell2World(Vector3Int cellPos)
    {
        return CellGrid.CellToWorld(cellPos);
    }

    public void LoadMap(string mapName)
    {
        DestroyMap();

        GameObject map = Managers.Resource.Instantiate(mapName);
        map.transform.position = Vector3.zero;
        map.name = $"@Map_{mapName}";

        Map = map;
        MapName = mapName;
        CellGrid = map.GetOrAddComponent<Grid>();

        ParseCollisionData(map, mapName);
    }

    public void DestroyMap()
    {
        _cells.Clear();

        if (Map != null)
        {
            Managers.Resource.Destroy(Map);
            Map = null;
        }
    }

    public Vector3Int? FindRandomCellPos(BaseObject self, int delta)
    {
        List<Vector3Int> randPos = new List<Vector3Int>();

        for (int x = -delta; x <= delta; x++)
        {
            for (int y = -delta; y <= delta; y++)
            {
                randPos.Add(new Vector3Int(self.CellPos.x + x, self.CellPos.y + y, 0));
            }
        }

        randPos.Shuffle();

        foreach (Vector3Int pos in randPos)
        {
            if (CanGo(self, pos))
                return pos;
        }

        return null;
    }

    public bool CanGo(BaseObject self, Vector3 worldPos, bool ignoreObjects = false)
    {
        return CanGo(self, World2Cell(worldPos), ignoreObjects);
    }

    public bool CanGo(BaseObject self, Vector3Int cellPos, bool ignoreObjects = false, int extraCell = 0)
    {
        int extraCells = extraCell;
        if (self != null)
            extraCells = self.ExtraCells;

        for (int dx = -extraCells; dx <= extraCells; dx++)
        {
            for (int dy = -extraCells; dy <= extraCells; dy++)
            {
                Vector3Int checkPos = new Vector3Int(cellPos.x + dx, cellPos.y + dy);

                if (CanGo_Internal(self, checkPos, ignoreObjects) == false)
                    return false;
            }
        }

        return true;
    }

    bool CanGo_Internal(BaseObject self, Vector3Int cellPos, bool ignoreObjects = false)
    {
        if (cellPos.x < MinX || cellPos.x > MaxX)
            return false;
        if (cellPos.y < MinY || cellPos.y > MaxY)
            return false;

        if (ignoreObjects == false)
        {
            BaseObject obj = GetObject(cellPos);
            if (obj != null && obj != self)
                return false;
        }

        int x = cellPos.x - MinX;
        int y = MaxY - cellPos.y;
        ECellCollisionType type = _collision[x, y];

        if (type == ECellCollisionType.None)
            return true;

        return false;
    }

    void ParseCollisionData(GameObject map, string mapName, string tilemap = "Tilemap_Collision")
    {
        GameObject collision = Utils.FindChild(map, tilemap, true);
        if (collision != null)
            collision.SetActive(false);

        // Collision ���� ����
        TextAsset txt = Managers.Resource.Load<TextAsset>($"{mapName}Collision");
        StringReader reader = new StringReader(txt.text);

        MinX = int.Parse(reader.ReadLine());
        MaxX = int.Parse(reader.ReadLine());
        MinY = int.Parse(reader.ReadLine());
        MaxY = int.Parse(reader.ReadLine());

        int xCount = MaxX - MinX + 1;
        int yCount = MaxY - MinY + 1;
        _collision = new ECellCollisionType[xCount, yCount];

        for (int y = 0; y < yCount; y++)
        {
            string line = reader.ReadLine();
            for (int x = 0; x < xCount; x++)
            {
                switch (line[x])
                {
                    case MAP_TOOL_WALL:
                        _collision[x, y] = ECellCollisionType.Wall;
                        break;
                    case MAP_TOOL_NONE:
                        _collision[x, y] = ECellCollisionType.None;
                        break;
                    default:
                        _collision[x, y] = ECellCollisionType.None;
                        break;
                }
            }
        }
    }

    public bool MoveTo(BaseObject obj, Vector3Int cellPos, bool forceMove = false)
    {
        if (CanGo(obj, cellPos) == false)
            return false;

        // ���� ��ǥ�� �ִ� ������Ʈ�� �о��ش�.
        // (��, ó�� ��û������ �ش� CellPos�� ������Ʈ�� ������ �ƴ� ���� ����)
        RemoveObject(obj);

        // �� ��ǥ�� ������Ʈ�� ����Ѵ�.
        AddObject(obj, cellPos);

        // �� ��ǥ �̵�
        obj.SetCellPos(cellPos, forceMove);

        return true;
    }

    #region Helpers

    public BaseObject GetObject(Vector3Int cellPos)
    {
        _cells.TryGetValue(cellPos, out BaseObject value);
        return value;
    }

    public BaseObject GetObject(Vector3 worldPos)
    {
        Vector3Int cellPos = World2Cell(worldPos);
        return GetObject(cellPos);
    }

    public void RemoveObject(BaseObject obj)
    {
        // ������ ��ǥ ����
        int extraCells = 0;
        if (obj != null)
            extraCells = obj.ExtraCells;

        Vector3Int cellPos = obj.CellPos;

        for (int dx = -extraCells; dx <= extraCells; dx++)
        {
            for (int dy = -extraCells; dy <= extraCells; dy++)
            {
                Vector3Int newCellPos = new Vector3Int(cellPos.x + dx, cellPos.y + dy);
                BaseObject prev = GetObject(newCellPos);

                if (prev == obj)
                    _cells[newCellPos] = null;
            }
        }
    }

    void AddObject(BaseObject obj, Vector3Int cellPos)
    {
        int extraCells = 0;
        if (obj != null)
            extraCells = obj.ExtraCells;

        for (int dx = -extraCells; dx <= extraCells; dx++)
        {
            for (int dy = -extraCells; dy <= extraCells; dy++)
            {
                Vector3Int newCellPos = new Vector3Int(cellPos.x + dx, cellPos.y + dy);

                BaseObject prev = GetObject(newCellPos);
                if (prev != null && prev != obj)
                    Debug.LogWarning($"AddObject ������");

                _cells[newCellPos] = obj;
            }
        }
    }

    public void ClearObjects()
    {
        _cells.Clear();
    }

    public Vector3Int GetFrontCellPos(EMoveDir dir)
    {
        if (dir == EMoveDir.None)
            return Vector3Int.zero;
        else
        {
            //delta�� 0,0�̾����ϱ� -1
            return _delta[(int)dir - 1];
        }
    }

    #endregion

    #region A* PathFinding

    public struct PQNode : IComparable<PQNode>
    {
        public int H; // Heuristic
        public Vector3Int CellPos;
        public int Depth;

        public int CompareTo(PQNode other)
        {
            if (H == other.H)
                return 0;
            return H < other.H ? 1 : -1;
        }
    }

    List<Vector3Int> _delta = new List<Vector3Int>()
    {
		//EMoveDir �̶� ���� ����
		new Vector3Int(1, 1, 0), // U
		new Vector3Int(-1, -1, 0), // D
		new Vector3Int(-1, 1, 0), // L
		new Vector3Int(1, -1, 0), // R
		new Vector3Int(0, 1, 0), // UL
		new Vector3Int(1, 0, 0), // UR
		new Vector3Int(-1, 0, 0), // DL
		new Vector3Int(0, -1, 0), // DR
    };

    public List<Vector3Int> FindPath(BaseObject self, Vector3Int startCellPos, Vector3Int destCellPos, int maxDepth = 10)
    {
        // ���ݱ��� ���� ���� �ĺ� ���.
        Dictionary<Vector3Int, int> best = new Dictionary<Vector3Int, int>();
        // ��� ���� �뵵.
        Dictionary<Vector3Int, Vector3Int> parent = new Dictionary<Vector3Int, Vector3Int>();

        // ���� �߰ߵ� �ĺ� �߿��� ���� ���� �ĺ��� ������ �̾ƿ��� ���� ����.
        PriorityQueue<PQNode> pq = new PriorityQueue<PQNode>(); // OpenList

        Vector3Int pos = startCellPos;
        Vector3Int dest = destCellPos;

        // destCellPos�� ���� ���ϴ��� ���� ����� �ַ�.
        Vector3Int closestCellPos = startCellPos;
        int closestH = (dest - pos).sqrMagnitude;

        // ������ �߰� (���� ����)
        {
            int h = (dest - pos).sqrMagnitude;
            pq.Push(new PQNode() { H = h, CellPos = pos, Depth = 1 });
            parent[pos] = pos;
            best[pos] = h;
        }

        while (pq.Count > 0)
        {
            // ���� ���� �ĺ��� ã�´�
            PQNode node = pq.Pop();
            pos = node.CellPos;

            // ������ ���������� �ٷ� ����.
            if (pos == dest)
                break;

            // �������� ���� ���� ����.
            if (node.Depth >= maxDepth)
                break;

            // �����¿� �� �̵��� �� �ִ� ��ǥ���� Ȯ���ؼ� �����Ѵ�.
            foreach (Vector3Int delta in _delta)
            {
                Vector3Int next = pos + delta;

                // �� �� ���� ��Ҹ� ��ŵ.
                if (CanGo(self, next) == false)
                    continue;

                // ���� ����
                int h = (dest - next).sqrMagnitude;

                // �� ���� �ĺ� ã�Ҵ���
                if (best.ContainsKey(next) == false)
                    best[next] = int.MaxValue;

                if (best[next] <= h)
                    continue;

                best[next] = h;

                pq.Push(new PQNode() { H = h, CellPos = next, Depth = node.Depth + 1 });
                parent[next] = pos;

                // ������������ �� ������, �׳��� ���� ���Ҵ� �ĺ� ���.
                if (closestH > h)
                {
                    closestH = h;
                    closestCellPos = next;
                }
            }
        }

        // ���� ����� �ֶ� ã��.
        if (parent.ContainsKey(dest) == false)
            return CalcCellPathFromParent(parent, closestCellPos);

        return CalcCellPathFromParent(parent, dest);
    }

    List<Vector3Int> CalcCellPathFromParent(Dictionary<Vector3Int, Vector3Int> parent, Vector3Int dest)
    {
        List<Vector3Int> cells = new List<Vector3Int>();

        if (parent.ContainsKey(dest) == false)
            return cells;

        Vector3Int now = dest;

        while (parent[now] != now)
        {
            cells.Add(now);
            now = parent[now];
        }

        cells.Add(now);
        cells.Reverse();

        return cells;
    }

    #endregion
}