using System.Collections;
using Data;
using Google.Protobuf.Protocol;
using System.Collections.Generic;
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
	private int _visionCells = 15;
	private LineRenderer _lineRenderer;
	private GameObject _moveCursor;
	public override bool IsMonitored => true;

    // 스킬 범위
    private LineRenderer _skillLineRenderer;
    private int _currentSkillId = 0;

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
    public bool IsAutoMode { get => _isAutoMode; set => _isAutoMode = value; }

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

	public void InitMyHero(MyHeroInfo myHeroInfo)
	{
		MyHeroInfo = myHeroInfo;
		ObjectId = myHeroInfo.HeroInfo.CreatureInfo.ObjectInfo.ObjectId;
		PosInfo = myHeroInfo.HeroInfo.CreatureInfo.ObjectInfo.PosInfo;
	}

	public override void SetInfo(int templateId)
	{
		base.SetInfo(templateId);
	}

    protected override void OnEnable()
    {
        base.OnEnable();
        Managers.Game.OnJoystickChanged -= HandleJoystickChanged;
        Managers.Game.OnJoystickChanged += HandleJoystickChanged;

        Managers.Event.AddEvent(EEventType.OnClickAttackButton, OnClickAttack);
        Managers.Event.AddEvent(EEventType.OnClickAutoButton, OnClickAutoMode);
        //Managers.Event.AddEvent(EEventType.OnClickPickupButton, OnClickPickup);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        Managers.Game.OnJoystickChanged -= HandleJoystickChanged;

        Managers.Event.RemoveEvent(EEventType.OnClickAttackButton, OnClickAttack);
        Managers.Event.RemoveEvent(EEventType.OnClickAutoButton, OnClickAutoMode);
        //Managers.Event.RemoveEvent(EEventType.OnClickPickupButton, OnClickPickup);
    }

    protected override void Awake()
    {
        base.Awake();
    }



    protected override void Start()
    {
        base.Start();

        InitLineRenderer();

        CameraController cc = Camera.main.GetOrAddComponent<CameraController>();
        if (cc != null)
            cc.Target = this;
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

    public void ClearMyHero()
    {
        DespawnMoveCursor();
    }
    #endregion

	#region Battle

	public override void OnDead()
	{
		base.OnDead();
		ClearMyHero();
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
			LookAtDest(_desiredDestPos.Value);
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

        // 2. Target이 없으면 리턴
        if (Target == null)
        {
            ObjectState = EObjectState.Idle;
            return;
        }

        // 3. 사용할 수 있는 스킬이 있으면 사용.
        Skill skill = GetNextUseSkill(Target);
        if (skill != null)
        {
            LookAtTarget(Target);
            ReqUseSkill(skill.TemplateId);
            return;
        }

        // 4. 공용 코드 실행.
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
	
	private bool _isMouseHeld = false;
	private float _timeSinceLastUpdate = 0f;
	private const float _updateInterval = 1f;
	void UpdateInput()
	{
		if (_joystickState == EJoystickState.Drag)
			return;

        //마우스 클릭 처리
        HandleMouseInput();

        //마우스 드래그시 1초마다 업데이트
        if (_isMouseHeld)
            UpdateMovementPeriodically();
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _isMouseHeld = true;
            _timeSinceLastUpdate = _updateInterval;
            UpdateMovement();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _isMouseHeld = false;
            _timeSinceLastUpdate = 0f;
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

    private void UpdateMovementPeriodically()
    {
        _timeSinceLastUpdate += Time.deltaTime;

        if (_timeSinceLastUpdate >= _updateInterval)
        {
            UpdateMovement();
            _timeSinceLastUpdate = 0f; // 타이머 초기화
        }
    }

    private void UpdateMovement()
    {
        if (IsPointerOverUIObject(Input.mousePosition))
            return;

        //ray
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, Mathf.Infinity);
        BaseObject obj = hit.collider?.gameObject.GetComponent<BaseObject>();

        if (obj != null)
            ProcessObjectClick(obj);
        else
            ProcessTerrainClick(mouseWorldPos);
    }

    private void ProcessObjectClick(BaseObject obj)
    {
        switch (obj.ObjectType)
        {
            case EGameObjectType.Hero:
            case EGameObjectType.Monster:
                DrawOutline(obj);
                break;
            case EGameObjectType.Npc:
                Npc npc = obj.GetComponent<Npc>();
                if (npc.Interaction.CanInteract())
                    npc.OnClickEvent();
                break;
        }
        DespawnMoveCursor();
    }

    private void ProcessTerrainClick(Vector2 mouseWorldPos)
    {
        ForceMove(mouseWorldPos);
        SpawnOrMoveCursor(mouseWorldPos);
    }

    private void SpawnOrMoveCursor(Vector2 position)
    {
        if (_objectState == EObjectState.Dead)
            return;

        if (_moveCursor == null)
        {
            _moveCursor = Managers.Object.Spawn("Cursor", isPooling: true);
        }
        _moveCursor.SetActive(true);
        _moveCursor.transform.position = position;
    }

    private void DespawnMoveCursor()
    {
        if (_moveCursor == null)
            return;
        _moveCursor.SetActive(false);
    }

    private void UpdateSendMovePacket()
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

    private void HandleJoystickChanged(EJoystickState joystickState, EMoveDir dir)
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
		// if (LerpCellPosCompleted == false)
		// 	return;
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
            Target.IsMonitored = true;
        }
    }

    private void OnClickPickup()
    {
    }
    #endregion

    #region Skill
    public Creature GetSkillTarget(Skill skill)
    {
        if (skill.SkillData.UseSkillTargetType == EUseSkillTargetType.Self)
            return this;

        return GetSelectedTarget();
    }

    public Creature GetSelectedTarget()
    {
        return SelectedObject as Creature;
    }

	int GetNextUseSkillDistance(Creature target)
	{
		// 1. 다음에 사용할 스킬 거리 반환.
		Skill skill = GetNextUseSkill(target);
		if (skill != null)
			return skill.GetSkillRange(target);

		// 2. 스킬이 다 쿨 돌고 있으면 기본 스킬 사거리로.
		Skill mainSkill = Managers.Skill.GetMainSkill();
		if (mainSkill != null)
			return mainSkill.GetSkillRange(target);

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

		Skill mainSkill = Managers.Skill.GetMainSkill();
		if (mainSkill.CanUseSkill(target) == ECanUseSkillFailReason.None)
			return mainSkill;

        return null;
    }

    public void ReqUseSkill(int templateId)
    {
        if (ObjectState == EObjectState.Dead)
            return;
        if (Managers.Data.SkillDict.TryGetValue(templateId, out SkillData skillData) == false)
            return;
        if (skillData.UseSkillTargetType != EUseSkillTargetType.Self && Target == null)
            return;

        _skillPacket.TemplateId = templateId;
        _currentSkillId = templateId;

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

        //스킬 범위 그리기
        StartCoroutine(DrawSkillRange(packet.TemplateId, packet.TargetId));

        //4. 스킬 범위에 있는 애들 모니터링 시작
        StartMonitoringSkillTargets(packet.TemplateId, packet.TargetId);
    }

    private void StartMonitoringSkillTargets(int templateId, int targetId)
    {
        if (Managers.Data.SkillDict.TryGetValue(templateId, out SkillData skillData) == false)
            return;

        Creature target = Managers.Object.FindCreatureById(targetId);
        if (target == null)
            return;

        Skill skill = Managers.Skill.GetSkill(templateId);
        if (skill == null)
            return;

        List<Creature> targets = skill.GatherSkillEffectTargets(this, skillData, target);
        foreach (var t in targets)
        {
            t.IsMonitored = true;
        }
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
        if (Managers.Data.BaseStatDict.TryGetValue(Level, out BaseStatData data))
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
        return level == Managers.Data.BaseStatDict.Count;
    }

    #endregion

    #region PacketHandler
    public void HandleRefreshStat(S_RefreshStat packet)
    {
        _creatureInfo.TotalStatInfo.MergeFrom((packet.TotalStatInfo));
        Managers.Event.TriggerEvent(EEventType.StatChanged);
    }

	public void HandleRewardValue(S_RewardValue rewardValue)
	{
		Gold += rewardValue.Gold;
		AddExp(rewardValue.Exp);
		Managers.Event.TriggerEvent(EEventType.CurrencyChanged);
		DamageFontController.AddDamageFont(rewardValue.Gold, transform, EFontType.Gold);
		DamageFontController.AddDamageFont(rewardValue.Exp, transform, EFontType.Exp);
	}
	#endregion

    #region OutLine
    private void DrawOutline(BaseObject obj)
    {
        // 기존에 선택된 오브젝트 아웃라인 제거.
        if (SelectedObject != null)
        {
            SelectedObject.OutLine.SetActive(false);
            SelectedObject = null;
        }

        // 아웃라인 추가.
        SelectedObject = obj;
        SelectedObject.OutLine.SetActive(true);

        ////TODO 타일(몬스터, NPC)클릭

        //if (hit.collider != null)
        //{
        //    BaseObject obj = hit.collider.gameObject.GetComponent<BaseObject>();
        //    if (obj != null)
        //    {
        //        // 아웃라인 추가.
        //        SelectedObject = obj;
        //        SelectedObject.OutLine.SetActive(true, Color.yellow);
        //        return;
        //    }
        //}
    }
    #endregion

    #region 디버깅
    void InitLineRenderer()
    {
        GameObject vision = new GameObject("VisionLineRenderer");
        vision.transform.parent = gameObject.transform;
        _lineRenderer = gameObject.AddComponent<LineRenderer>();
        _lineRenderer.startWidth = _lineWidth;
        _lineRenderer.endWidth = _lineWidth;
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.startColor = _lineColor;
        _lineRenderer.endColor = _lineColor;
        _lineRenderer.sortingOrder = 800;

        GameObject skillLineObject = new GameObject("SkillLineRenderer");
        skillLineObject.transform.parent = gameObject.transform;
        _skillLineRenderer = skillLineObject.AddComponent<LineRenderer>();
        _skillLineRenderer.startWidth = _lineWidth;
        _skillLineRenderer.endWidth = _lineWidth;
        _skillLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _skillLineRenderer.startColor = Color.blue;
        _skillLineRenderer.endColor = _lineColor;
        _skillLineRenderer.sortingOrder = 800;
    }
    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (Managers.Scene.CurrentScene.TestMode == false)
            return;

        Gizmos.color = Color.red;
        Vector3 textPosition = transform.position + Vector3.up * 3.5f + Vector3.left * 0.5f;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;
        style.fontSize = 25;
        UnityEditor.Handles.Label(textPosition, ObjectState.ToString(), style);
    }
    #endif

    void DrawVisionCells()
    {
        if (Managers.Scene.CurrentScene.TestMode == false)
        {
            _lineRenderer.positionCount = 0;
            return;
        }

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

    IEnumerator DrawSkillRange(int templateId, int targetObjectId)
    {
        if (Managers.Data.SkillDict.TryGetValue(templateId, out SkillData skillData) == false)
            yield break;
        if (Managers.Scene.CurrentScene.TestMode == false)
            yield break;
        int range = skillData.GatherTargetRange;

        Vector3Int pos = GetSkillRangePivotPos(skillData, targetObjectId).Value;

        Vector3Int bottomLeft = pos + new Vector3Int(-range, -range, 0);
        Vector3Int bottomRight = pos + new Vector3Int(range, -range, 0);
        Vector3Int topLeft = pos + new Vector3Int(-range, range, 0);
        Vector3Int topRight = pos + new Vector3Int(range, range, 0);

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

        _skillLineRenderer.positionCount = positions.Length;
        _skillLineRenderer.SetPositions(positions);

        yield return new WaitForSeconds(0.5f);

        // 라인 클리어
        _skillLineRenderer.positionCount = 0;
    }

    private Vector3Int? GetSkillRangePivotPos(SkillData skillData, int targetObjectId)
    {
        Vector3Int pos = Vector3Int.zero;
        switch (skillData.UseSkillTargetType)
        {
            case EUseSkillTargetType.Self:
                return CellPos;
                break;
            case EUseSkillTargetType.Other:
                return Managers.Object.FindCreatureById(targetObjectId).CellPos;
            case EUseSkillTargetType.Any:
                return null;
        }

        return null;
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
