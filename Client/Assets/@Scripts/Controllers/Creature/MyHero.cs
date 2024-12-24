using System;
using Data;
using Google.Protobuf.Protocol;
using System.Collections.Generic;
using Scripts.Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

public class MyHero : Hero
{
    //Temp
    public override float MoveSpeed => TotalStat.Speed;

    // VisionCells �����׸���
    private Color _lineColor = Color.red;
    private float _lineWidth = 0.1f;
    private int _visionCells = 10;
    private LineRenderer _lineRenderer;
    private GameObject _moveCursor;

    private Monster _target;
    // �̵� ��Ŷ ���� ���� (������ dirty flag)
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
    private EMoveDir _joystickDir;

    [SerializeField] private EHeroMoveState _heroMoveState = EHeroMoveState.None;
    public EHeroMoveState HeroMoveState
    {
        get => _heroMoveState;
        set
        {
            if (_heroMoveState == value)
                return;

            if (_heroMoveState != value)
            {
                if (value == EHeroMoveState.ForceMove)
                {
                    _isAutoMode = false;
                    CancelWait();
                    _target = null;
                    // Skills.CurrentSkill.CancelSkill();
                }
            }
            _heroMoveState = value;
        }
    }

    private bool _isAutoMode = false;
    [SerializeField] private EJoystickState _joystickState;


    #region LifeCycle

    public override void SetInfo(int templatedId)
    {
        base.SetInfo(templatedId);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        Managers.Game.OnJoystickChanged -= HandleJoystickChanged;
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        Managers.Game.OnJoystickChanged -= HandleJoystickChanged;
        Managers.Game.OnJoystickChanged += HandleJoystickChanged;
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

    // TEMP : ���ǿ� �������� �ڵ�. ��ġ�� �κ� ��� ������ �÷����� ����.
    protected override void Update()
    {
        // �Է� ó��
        UpdateInput();

        // �⺻������ ��� ��ü�� ĭ ������ ����������, Ŭ�󿡼� '������' �����̴� ���� ó���� ���ش�.
        UpdateLerpToCellPos(MoveSpeed, true);

        // ��� ��ǥ�� �ٲ���ٸ� ������ ����.
        UpdateSendMovePacket();

        // ����� �뵵
        DrawVisionCells();
    }
    #endregion

    #region AI (FSM)
    protected override void UpdateIdle()
    {
        base.UpdateIdle();

        // 1. �̵� ��ġ�� ����.
        if (_desiredDestPos.HasValue)
        {
            // �̵�
            EFindPathResult res = FindPathToCellPos(_desiredDestPos.Value, Define.HERO_DEFAULT_MOVE_DEPTH, out List<Vector3Int> path);
            if (res == EFindPathResult.Success)
            {
                DestPos = path[1];
                return;
            }
        }

        if (LerpCellPosCompleted == false)
        {
            ObjectState = EObjectState.Move;
            return;
        }
    }

    protected override void UpdateMove()
    {   
        // base.UpdateMove();
        if (_heroMoveState == EHeroMoveState.ForceMove)
        {
            // �̵�
            if (_desiredDestPos.HasValue)
            {
                List<Vector3Int> path = new List<Vector3Int>();
                EFindPathResult res = FindPathToCellPos(_desiredDestPos.Value, HERO_DEFAULT_MOVE_DEPTH, out path);
                if (res == EFindPathResult.Success)
                {
                    DestPos = path[1];
                    return;
                }
            }
        }

        // �̵� ��������.
        if (LerpCellPosCompleted)
        {
            ObjectState = EObjectState.Idle;
            _desiredDestPos = null;
            DespawnMoveCursor();
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

    #region �̵� ����ȭ
    protected override void UpdateAnimation()
    {
        base.UpdateAnimation();
        switch (ObjectState)
        {
            case EObjectState.Idle:
                HeroMoveState = EHeroMoveState.None;
                break;
            case EObjectState.Skill:
                HeroMoveState = EHeroMoveState.None;
                break;
            case EObjectState.Move:
                break;
            case EObjectState.Dead:
                break;
        }
    }
    void UpdateInput()
    {
        if (Input.GetMouseButton(0))
        {
            if (_joystickState == EJoystickState.Drag)
                return;

            // UI Ŭ���� ����
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            //
            //TODO Ÿ��(����, NPC)Ŭ��

            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _desiredDestPos = mouseWorldPos;
            HeroMoveState = EHeroMoveState.ForceMove;

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

    void DespawnMoveCursor()
    {
        Managers.Resource.Destroy(_moveCursor);
        _moveCursor = null;
    }

    void UpdateSendMovePacket()
    {
        if (_sendMovePacket)
        {
            C_Move movePacket = new C_Move() { PosInfo = new PositionInfo() };
            movePacket.PosInfo.MergeFrom(PosInfo);
            movePacket.PosInfo.State = EObjectState.Move;
            movePacket.PosInfo.PosX = DestPos.x;
            movePacket.PosInfo.PosY = DestPos.y;
            Managers.Network.GameServer.Send(movePacket);
            _sendMovePacket = false;

            Debug.Log($@"@>> C_Move , {DestPos.x},{DestPos.y}");
        }
    }

    void HandleJoystickChanged(EJoystickState joystickState, EMoveDir dir)
    {
        _joystickState = joystickState;
        switch (joystickState)
        {
            case EJoystickState.None:
                break;
            case EJoystickState.PointerDown:
                break;
            case EJoystickState.Drag:
                DespawnMoveCursor();
                ForceMove(dir);
                break;
            case EJoystickState.PointerUp:
                break;
            case EJoystickState.Attack:
                Debug.Log("Attack Button");
                break;
            case EJoystickState.Auto:
                Debug.Log("Auto Button");
                _isAutoMode = !_isAutoMode;
                break;
            case EJoystickState.Pickup:
                Debug.Log("Pickup Button");
                break;
        }
    }

    private void ForceMove(EMoveDir dir)
    {
        if (dir == EMoveDir.None)
        {
            return;
        }

        if (LerpCellPosCompleted == false)
        {
            return;
        }

        HeroMoveState = EHeroMoveState.ForceMove;
        MoveDir = dir;
        Vector3Int dest = CellPos + Managers.Map.GetFrontCellPos(dir);
        _desiredDestPos = Managers.Map.Cell2World(dest);
        _isAutoMode = false;
    }
    #endregion

        #region �����
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
            obj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f); // ũ�� ����
            obj.transform.SetParent(parentObject.transform); // �θ� ����

        }
    }
    #endregion
}
