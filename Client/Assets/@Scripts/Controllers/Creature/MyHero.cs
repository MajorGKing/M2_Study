using Data;
using Google.Protobuf.Protocol;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MyHero : Hero
{
    // VisionCells 범위그리기
    private Color _lineColor = Color.red;
    private float _lineWidth = 0.1f;
    private int _visionCells = 10;
    private LineRenderer _lineRenderer;
    private GameObject _moveCursor;

    // 이동 패킷 전송 관련 (일종의 dirty flag)
    protected bool _sendMovePacket = false;
    Vector3Int _destPos;
    public Vector3Int DestPos
    {
        get { return _destPos; }
        set
        {
            if (_destPos == value)
                return;

            _destPos = value;
            _sendMovePacket = true;
        }
    }

    Vector3? _desiredDestPos;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        _lineRenderer = gameObject.GetOrAddComponent<LineRenderer>();
        _lineRenderer.startWidth = _lineWidth;
        _lineRenderer.endWidth = _lineWidth;
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.startColor = _lineColor;
        _lineRenderer.endColor = _lineColor;
        _lineRenderer.sortingOrder = 800;

        CameraController cc = Camera.main.GetOrAddComponent<CameraController>();
        if (cc != null)
            cc.Target = this;

        DrawCollision();
    }

    // TEMP : 강의용 직관적인 코드. 겹치는 부분 상속 구조로 올려버릴 예정.
    protected override void Update()
    {
        // 입력 처리
        UpdateInput();

        // FSM 방식의 상태 관리.
        UpdateAI();

        // 기본적으로 모든 물체는 칸 단위로 움직이지만, 클라에서 '스르륵' 움직이는 보정 처리를 해준다.
        UpdateLerpToCellPos(MoveSpeed, true);

        // 희망 좌표가 바뀌었다면 서버에 전송.
        UpdateSendMovePacket();

        // 디버그 용도
        DrawVisionCells();
    }

    #region AI (FSM)
    protected override void UpdateIdle()
    {
        base.UpdateIdle();

        // 1. 이동 위치를 찍음.
        if (_desiredDestPos.HasValue)
        {
            // 이동
            EFindPathResult res = FindPathToCellPos(_desiredDestPos.Value, Define.HERO_DEFAULT_MOVE_DEPTH, out List<Vector3Int> path);
            if (res == EFindPathResult.Success)
            {
                DestPos = path[1];
                return;
            }
        }

        // 2. 공격 대상을 찾았다.

        if (LerpCellPosCompleted == false)
        {
            ObjectState = EObjectState.Move;
            return;
        }
    }

    protected override void UpdateMove()
    {
        // base.UpdateMove();

        // 1. 이동 위치에 도착.
        if (_desiredDestPos.HasValue == false)
        {
            ObjectState = EObjectState.Idle;
            return;
        }
        else
        {
            // 이동
            List<Vector3Int> path = new List<Vector3Int>();
            EFindPathResult res = FindPathToCellPos(_desiredDestPos.Value, Define.HERO_DEFAULT_MOVE_DEPTH, out path);
            if (res == EFindPathResult.Success)
            {
                DestPos = path[1];
                return;
            }
        }

        // 이동 끝났으면.
        if (LerpCellPosCompleted)
        {
            ObjectState = EObjectState.Idle;
            _desiredDestPos = null;
            Managers.Resource.Destroy(_moveCursor);
            _moveCursor = null;
            return;
        }
    }

    protected override void UpdateSkill()
    {
        base.UpdateSkill();

    }

    protected override void UpdateDead()
    {
        base.UpdateDead();

    }

    #endregion

    #region 이동 동기화
    void UpdateInput()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _desiredDestPos = mouseWorldPos;

            SpawnOrMoveCursor(mouseWorldPos);
        }
    }

    void SpawnOrMoveCursor(Vector2 position)
    {
        if (_moveCursor != null)
        {
            _moveCursor.transform.position = position;
        }
        else
        {
            _moveCursor = Managers.Resource.Instantiate("Cursor", pooling: true);
            _moveCursor.transform.position = position;
        }
    }

    void UpdateSendMovePacket()
    {
        if (_sendMovePacket)
        {
            C_Move movePacket = new C_Move() { PosInfo = new PositionInfo() };
            movePacket.PosInfo.MergeFrom(PosInfo);
            movePacket.PosInfo.PosX = DestPos.x;
            movePacket.PosInfo.PosY = DestPos.y;
            Managers.Network.GameServer.Send(movePacket);
            _sendMovePacket = false;

            Debug.Log($@"@>> C_Move , {DestPos.x},{DestPos.y}");
        }
    }
    #endregion

    #region 디버깅
    void DrawVisionCells()
    {
        Vector3Int bottomLeft = CellPos + new Vector3Int(-_visionCells, -_visionCells, 0);
        Vector3Int bottomRight = CellPos + new Vector3Int(_visionCells, -_visionCells, 0);
        Vector3Int topLeft = CellPos + new Vector3Int(-_visionCells, _visionCells, 0);
        Vector3Int topRight = CellPos + new Vector3Int(_visionCells, _visionCells, 0);

        Vector3 worldBottomLeft = Managers.Map.Cell2World(bottomLeft);
        Vector3 worldBottomRight = Managers.Map.Cell2World(bottomRight);
        Vector3 worldTopLeft = Managers.Map.Cell2World(topLeft);
        Vector3 worldTopRight = Managers.Map.Cell2World(topRight);

        Vector3[] positions = new Vector3[5];
        positions[0] = worldBottomLeft;
        positions[1] = worldBottomRight;
        positions[2] = worldTopRight;
        positions[3] = worldTopLeft;
        positions[4] = worldBottomLeft;

        _lineRenderer.positionCount = positions.Length;
        _lineRenderer.SetPositions(positions);
    }

    void DrawCollision()
    {
        for (int y = Managers.Map.MinY; y < Managers.Map.MaxY; y++)
        {
            for (int x = Managers.Map.MinX; x < Managers.Map.MaxX; x++)
            {
                DrawObject(new(x, y, 0));
            }
        }
    }

    void DrawObject(Vector3Int tilePos)
    {
        Vector3 worldPos = Managers.Map.Cell2World(tilePos);

        if (Managers.Map.CanGo(this, tilePos))
        {
        }
        else
        {
            GameObject parentObject = GameObject.Find("Test");
            if (parentObject == null)
            {
                parentObject = new GameObject("Test");
            }

            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.transform.position = worldPos;
            obj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f); // 크기 조정
            obj.transform.SetParent(parentObject.transform); // 부모 설정

        }
    }
    #endregion
}
