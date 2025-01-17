using System;
using System.Collections;
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
    public override float MoveSpeed => TotalStat.MoveSpeed;

    // VisionCells 범위그리기
    private Color _lineColor = Color.red;
    private float _lineWidth = 0.1f;
    private int _visionCells = 10;
    private LineRenderer _lineRenderer;
    private GameObject _moveCursor;

    public override HeroInfo HeroInfo => MyHeroInfo.HeroInfo;
    public MyHeroInfo MyHeroInfo { get; set; }
    public BaseObject SelectedObject { get; private set; }

    public Creature Target { get; set; }
    //private Creature Target { get => Target; set => Target = value; }

    // 이동 패킷 전송 관련 (일종의 dirty flag)
    protected bool _sendMovePacket = false;

    // 스킬 패킷 전송
    C_Skill _skillPacket = new C_Skill();

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

            //if (_heroMoveState != value)
            //{
            //    if (value == EHeroMoveState.ForceMove)
            //    {
            //        _isAutoMode = false;
            //        CancelWait();
            //        _target = null;
            //        // Skills.CurrentSkill.CancelSkill();
            //    }
            //}
            _heroMoveState = value;
        }
    }

    private bool _isAutoMode = false;
    [SerializeField] private EJoystickState _joystickState;

    #region MyHeroInfo Values
    public int Level
    {
        get { return MyHeroInfo.HeroInfo.Level; }
        private set { MyHeroInfo.HeroInfo.Level = value; }
    }

    public int Gold
    {
        get { return MyHeroInfo.CurrencyInfo.Gold; }
        private set { MyHeroInfo.CurrencyInfo.Gold = value; }
    }

    public int Dia
    {
        get { return MyHeroInfo.CurrencyInfo.Dia; }
        private set { MyHeroInfo.CurrencyInfo.Dia = value; }
    }

    public int Exp
    {
        get { return MyHeroInfo.Exp; }
        private set { MyHeroInfo.Exp = value; }
    }
    #endregion

    #region LifeCycle

    public override void SetInfo(int templatedId)
    {
        base.SetInfo(templatedId);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        Managers.Game.OnJoystickChanged -= HandleJoystickChanged;
        Managers.Game.OnJoystickChanged += HandleJoystickChanged;

        Managers.Event.AddEvent(EEventType.OnClickAttackButton, OnClickAttack);
        Managers.Event.AddEvent(EEventType.OnClickAutoButton, OnClickAutoMode);
        Managers.Event.AddEvent(EEventType.OnClickPickupButton, OnClickPickup);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        Managers.Game.OnJoystickChanged -= HandleJoystickChanged;

        Managers.Event.RemoveEvent(EEventType.OnClickAttackButton, OnClickAttack);
        Managers.Event.RemoveEvent(EEventType.OnClickAutoButton, OnClickAutoMode);
        Managers.Event.RemoveEvent(EEventType.OnClickPickupButton, OnClickPickup);
    }

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

        AddHpBar();
        //DrawCollision();

    }

    // TEMP : 강의용 직관적인 코드. 겹치는 부분 상속 구조로 올려버릴 예정.
    protected override void Update()
    {
        // 입력 처리
        UpdateInput();

        // 기본적으로 모든 물체는 칸 단위로 움직이지만, 클라에서 '스르륵' 움직이는 보정 처리를 해준다.
        UpdateLerpToCellPos(MoveSpeed, true);

        // 희망 좌표가 바뀌었다면 서버에 전송.
        UpdateSendMovePacket();

        // 디버그 용도
        DrawVisionCells();
    }
    #endregion

    #region AI (FSM)
    bool ChaseTargetOrUseAvailableSkill()
    {
        // 1-1. 타겟이 여전히 유효한지 확인.
        if (Target.IsValid() == false)
        {
            ObjectState = EObjectState.Idle;
            return true;
        }

        _desiredDestPos = null;

        int dist = GetDistance(Target);
        int skillRange = GetNextUseSkillDistance(Target);

        // 1-2. 너무 멀면 타겟에 가까이 다가감.
        if (dist > skillRange)
        {
            EFindPathResult res = FindPathToCellPos(Target.CellPos, HERO_DEFAULT_MOVE_DEPTH, out List<Vector3Int> path);
            if (res == EFindPathResult.Success)
            {
                DestPos = path[1];
                ObjectState = EObjectState.Move;
                HeroMoveState = EHeroMoveState.TargetMonster;
                return true;
            }
        }

        // 1-3. 사용할 수 있는 스킬이 있으면 사용.
        Skill skill = GetNextUseSkill(Target);
        if (skill != null)
        {
            ObjectState = EObjectState.Skill;
            return true;
        }

        return false;
    }
    protected override void UpdateIdle()
    {
        // 1. 자동 사냥 모드 처리.
        if (_isAutoMode)
        {
            // 1-1. 타겟 검색.
            if (Target.IsValid() == false)
                Target = Managers.Object.FindClosestMonster();

            // 1-2. 타겟이 없으면. 주기적으로 정찰.
            if (Target.IsValid() == false)
            {
                Vector3Int? cellPos = Managers.Map.FindRandomCellPos(this, 10);

                if (cellPos != null)
                    _desiredDestPos = Managers.Map.Cell2World(cellPos.Value);
            }
        }

        // 2. 타겟이 있다면.
        if (Target.IsValid())
        {
            // 2-1. 공용 코드 실행.
            if (ChaseTargetOrUseAvailableSkill())
                return;
        }

        // 3. 이동 목적지가 결정됨.
        if (_desiredDestPos.HasValue)
        {
            EFindPathResult destRes = FindPathToCellPos(_desiredDestPos.Value, HERO_DEFAULT_MOVE_DEPTH, out List<Vector3Int> destPath);
            if (destRes == EFindPathResult.Success)
            {
                DestPos = destPath[1];
                ObjectState = EObjectState.Move;
                HeroMoveState = EHeroMoveState.MoveToDesiredPos;
                return;
            }
        }
    }

    protected override void UpdateMove()
    {
        if (LerpCellPosCompleted == false)
            return;

        // 1. 타겟이 있음.
        if (HeroMoveState == EHeroMoveState.TargetMonster)
        {
            // 1-1. 공용 코드 실행.
            if (ChaseTargetOrUseAvailableSkill())
                return;
        }
        else
        {
            // 2. 목적지 향해 이동.
            if (_desiredDestPos.HasValue)
            {
                EFindPathResult res = FindPathToCellPos(_desiredDestPos.Value, HERO_DEFAULT_MOVE_DEPTH, out List<Vector3Int> path);
                if (res == EFindPathResult.Success)
                {
                    DestPos = path[1];
                    return;
                }
            }
        }

        // 3. 이동 불가.
        ObjectState = EObjectState.Idle;
        _desiredDestPos = null;
        DespawnMoveCursor();
    }

    protected override void UpdateSkill()
    {
        // 1. 스킬 사용중이면 리턴
        if (_coWait != null)
            return;

        // 2. 사용할 수 있는 스킬이 있으면 사용.
        Skill skill = GetNextUseSkill(Target);
        if (skill != null)
        {
            LookAtTarget(Target);
            ReqUseSkill(skill.TemplateId);
            return;
        }

        // 3. 공용 코드 실행.
        if (ChaseTargetOrUseAvailableSkill())
            return;

    }

    protected override void UpdateDead()
    {
        base.UpdateDead();

    }

    #endregion

    #region 이동 동기화
    protected override void UpdateAnimation()
    {
        base.UpdateAnimation();

        if (ObjectState != EObjectState.Move)
            HeroMoveState = EHeroMoveState.None;
    }
    void UpdateInput()
    {
        if (Input.GetMouseButton(0))
        {
            if (_joystickState == EJoystickState.Drag)
                return;

            // UI 클릭을 무시
            if (IsPointerOverUIObject(Input.mousePosition))
                return;

            // 기존에 선택된 오브젝트 아웃라인 제거.
            if (SelectedObject != null)
            {
                SelectedObject.OutLine.Clear();
                SelectedObject = null;
            }
            //
            //TODO 타일(몬스터, NPC)클릭
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, Mathf.Infinity);
            if (hit.collider != null)
            {
                BaseObject obj = hit.collider.gameObject.GetComponent<BaseObject>();
                if (obj != null)
                {
                    // 아웃라인 추가.
                    SelectedObject = obj;
                    SelectedObject.OutLine.SetActive(true, Color.yellow);
                    return;
                }
            }

            // 여기까지 왔으면 지형 클릭.
            ForceMove(mouseWorldPos);

            SpawnOrMoveCursor(mouseWorldPos);
        }
    }

    public bool IsPointerOverUIObject(Vector2 touchPos)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = touchPos;
        List<RaycastResult> results = new List<RaycastResult>();

        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        return results.Count > 0;
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
            _sendMovePacket = false;

            C_Move movePacket = new C_Move() { PosInfo = new PositionInfo() };
            movePacket.PosInfo.MergeFrom(PosInfo);
            movePacket.PosInfo.State = EObjectState.Move;
            movePacket.PosInfo.PosX = DestPos.x;
            movePacket.PosInfo.PosY = DestPos.y;
            
            Managers.Network.GameServer.Send(movePacket);

            //Debug.Log($@"@>> C_Move , {DestPos.x},{DestPos.y}");
        }
    }

    void HandleJoystickChanged(EJoystickState joystickState, EMoveDir dir)
    {
        _joystickState = joystickState;


        if (joystickState == EJoystickState.Drag)
        {
            DespawnMoveCursor();
            ForceMove(dir);
        }
    }

    private void ForceMove(EMoveDir dir)
    {
        if (dir == EMoveDir.None)
            return;

        if (LerpCellPosCompleted == false)
            return;

        MoveDir = dir;
        Vector3Int dest = CellPos + Managers.Map.GetFrontCellPos(dir);
        Vector3 pos = Managers.Map.Cell2World(dest);
        ForceMove(pos);
    }

    private void ForceMove(Vector3 pos)
    {
        if (LerpCellPosCompleted == false)
            return;

        _desiredDestPos = pos;
        _isAutoMode = false;
        Target = null;

        CancelWait();
    }
    #endregion

    #region OnClick
    private void OnClickAutoMode()
    {
        _isAutoMode = !_isAutoMode;
    }

    private void OnClickAttack()
    {
        if(SelectedObject == null)
        {
            Managers.UI.ShowToast("TODO 대상이 없습니다.");
            return;
        }

        Creature target = SelectedObject as Creature;
        if (target.IsValid())
        {
            Target = target;
        }
    }

    private void OnClickPickup()
    {
    }
    #endregion

    #region Skill

    int GetNextUseSkillDistance(Creature target)
    {
        // 1. 다음에 사용할 스킬 거리 반환.
        Skill skill = GetNextUseSkill(target);
        if (skill != null)
            return skill.SkillData.SkillRange;

        // 2. 스킬이 다 쿨 돌고 있으면 기본 스킬 사거리로.
        Skill mainSkill = Managers.Skill.GetMainSkill();
        if (mainSkill != null)
            return mainSkill.SkillData.SkillRange;

        return 0;
    }

    Skill GetNextUseSkill(Creature target)
    {
        List<Skill> skills = Managers.Skill.GetAllSkills(excludeMainSkill: true);
        foreach (Skill skill in skills)
        {
            if (skill.CanUseSkill(target) == ECanUseSkillFailReason.None)
                return skill;
        }

        Skill mainSKill = Managers.Skill.GetMainSkill();
        if (mainSKill.CanUseSkill(target) == ECanUseSkillFailReason.None)
            return mainSKill;

        return null;
    }

    public void ReqUseSkill(int templateId)
    {
        if (ObjectState == EObjectState.Dead)
            return;
        if (Managers.Data.SkillDic.TryGetValue(templateId, out SkillData skillData) == false)
            return;
        if (skillData.UseSkillTargetType != EUseSkillTargetType.Self && Target == null)
            return;

        _skillPacket.TemplateId = templateId;

        if (skillData.UseSkillTargetType == EUseSkillTargetType.Self)
            _skillPacket.TargetId = ObjectId;
        else
            _skillPacket.TargetId = Target.ObjectId;

        Managers.Network.GameServer.Send(_skillPacket);

        StartWait(0.1f);
    }

    public override void HandleSkillPacket(S_Skill packet)
    {
        base.HandleSkillPacket(packet);

        Managers.Skill.UpdateCooltime(packet.TemplateId);
    }
    #endregion

    #region Level System
    public void AddExp(int amount)
    {
        if (IsMaxLevel())
            return;

        Exp += amount;
        while (!IsMaxLevel() && Exp >= GetExpToNextLevel())
        {
            Exp -= GetExpToNextLevel();
            Level++;
        }
    }

    public bool CanLevelUp()
    {
        return (GetExpToNextLevel() - Exp <= 0);
    }

    public float GetExpNormalized()
    {
        if (IsMaxLevel())
        {
            return 1f;
        }

        return (float)Exp / GetExpToNextLevel();
    }

    public int GetExpToNextLevel()
    {
        if (Managers.Data.BaseStatDic.TryGetValue(Level, out BaseStatData data))
        {
            return data.Exp;
        }
        else
        {
            return 100;
        }
    }

    public bool IsMaxLevel()
    {
        return IsMaxLevel(Level);
    }

    public bool IsMaxLevel(int level)
    {
        return level == Managers.Data.BaseStatDic.Count;
    }

    #endregion

    #region PacketHandler
    public void HandleChangeStat(S_ChangeStat packet)
    {
        _creatureInfo.TotalStatInfo.MergeFrom((packet.TotalStatInfo));
        Managers.Event.TriggerEvent(EEventType.StatChanged);
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
