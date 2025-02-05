using System;
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

    // VisionCells �����׸���
    private Color _lineColor = Color.red;
    private float _lineWidth = 0.1f;
    private int _visionCells = 10;
    private LineRenderer _lineRenderer;
    private GameObject _moveCursor;

    // ��ų ����
    private LineRenderer _skillLineRenderer;
    private int _currentSkillId = 0;

    public override HeroInfo HeroInfo => MyHeroInfo.HeroInfo;
    public MyHeroInfo MyHeroInfo { get; set; }
    public BaseObject SelectedObject { get; private set; }

    public Creature Target { get; set; }
    //private Creature Target { get => Target; set => Target = value; }

    // �̵� ��Ŷ ���� ���� (������ dirty flag)
    protected bool _sendMovePacket = false;

    // ��ų ��Ŷ ����
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

        InitLineRenderer();

        CameraController cc = Camera.main.GetOrAddComponent<CameraController>();
        if (cc != null)
            cc.Target = this;

        AddHpBar();
        //DrawCollision();

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
    bool ChaseTargetOrUseAvailableSkill()
    {
        // 1-1. Ÿ���� ������ ��ȿ���� Ȯ��.
        if (Target.IsValid() == false)
        {
            ObjectState = EObjectState.Idle;
            return true;
        }

        _desiredDestPos = null;

        int dist = GetDistance(Target);
        int skillRange = GetNextUseSkillDistance(Target);

        // 1-2. �ʹ� �ָ� Ÿ�ٿ� ������ �ٰ���.
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

        // 1-3. ����� �� �ִ� ��ų�� ������ ���.
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
        // 1. �ڵ� ��� ��� ó��.
        if (_isAutoMode)
        {
            // 1-1. Ÿ�� �˻�.
            if (Target.IsValid() == false)
                Target = Managers.Object.FindClosestMonster();

            // 1-2. Ÿ���� ������. �ֱ������� ����.
            if (Target.IsValid() == false)
            {
                Vector3Int? cellPos = Managers.Map.FindRandomCellPos(this, 10);

                if (cellPos != null)
                    _desiredDestPos = Managers.Map.Cell2World(cellPos.Value);
            }
        }

        // 2. Ÿ���� �ִٸ�.
        if (Target.IsValid())
        {
            // 2-1. ���� �ڵ� ����.
            if (ChaseTargetOrUseAvailableSkill())
                return;
        }

        // 3. �̵� �������� ������.
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

        // 1. Ÿ���� ����.
        if (HeroMoveState == EHeroMoveState.TargetMonster)
        {
            // 1-1. ���� �ڵ� ����.
            if (ChaseTargetOrUseAvailableSkill())
                return;
        }
        else
        {
            // 2. ������ ���� �̵�.
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

        // 3. �̵� �Ұ�.
        ObjectState = EObjectState.Idle;
        _desiredDestPos = null;
        DespawnMoveCursor();
    }

    protected override void UpdateSkill()
    {
        // 1. ��ų ������̸� ����
        if (_coWait != null)
            return;

        // 2. ����� �� �ִ� ��ų�� ������ ���.
        Skill skill = GetNextUseSkill(Target);
        if (skill != null)
        {
            LookAtTarget(Target);
            ReqUseSkill(skill.TemplateId);
            return;
        }

        // 3. ���� �ڵ� ����.
        if (ChaseTargetOrUseAvailableSkill())
            return;

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

        if (ObjectState != EObjectState.Move)
            HeroMoveState = EHeroMoveState.None;
    }
    void UpdateInput()
    {
        if (Input.GetMouseButton(0) == false)
            return;

        if (_joystickState == EJoystickState.Drag)
            return;

        // UI Ŭ���� ����
        if (IsPointerOverUIObject(Input.mousePosition))
            return;

        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, Mathf.Infinity);

        BaseObject obj = null;
        if (hit.collider != null)
            obj = hit.collider.gameObject.GetComponent<BaseObject>();

        // 1. ������Ʈ Ŭ��.
        if (obj != null)
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
            return;
        }

        // 2. ���� Ŭ��.
        ForceMove(mouseWorldPos);
        SpawnOrMoveCursor(mouseWorldPos);

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
            Managers.UI.ShowToast("TODO ����� �����ϴ�.");
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
        // 1. ������ ����� ��ų �Ÿ� ��ȯ.
        Skill skill = GetNextUseSkill(target);
        if (skill != null)
            return skill.SkillData.SkillRange;

        // 2. ��ų�� �� �� ���� ������ �⺻ ��ų ��Ÿ���.
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

        //��ų ���� �׸���
        StartCoroutine(DrawSkillRange(packet.TemplateId, packet.TargetId));
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
    public void HandleChangeStat(S_ChangeStat packet)
    {
        _creatureInfo.TotalStatInfo.MergeFrom((packet.TotalStatInfo));
        Managers.Event.TriggerEvent(EEventType.StatChanged);
    }

    public void HandleRewardValue(S_RewardValue rewardValue)
    {
        Gold += rewardValue.Gold;
        AddExp(rewardValue.Exp);
        Managers.Event.TriggerEvent(EEventType.CurrencyChanged);
        DamageFontController.AddDamageFont(rewardValue.Gold, transform, EDamageType.Gold);
        DamageFontController.AddDamageFont(rewardValue.Exp, transform, EDamageType.Exp);
    }
    #endregion

    #region OutLine
    private void DrawOutline(BaseObject obj)
    {
        // ������ ���õ� ������Ʈ �ƿ����� ����.
        if (SelectedObject != null)
        {
            SelectedObject.OutLine.Clear();
            SelectedObject = null;
        }

        // �ƿ����� �߰�.
        SelectedObject = obj;
        SelectedObject.OutLine.SetActive(true, Color.yellow);

        ////TODO Ÿ��(����, NPC)Ŭ��

        //if (hit.collider != null)
        //{
        //    BaseObject obj = hit.collider.gameObject.GetComponent<BaseObject>();
        //    if (obj != null)
        //    {
        //        // �ƿ����� �߰�.
        //        SelectedObject = obj;
        //        SelectedObject.OutLine.SetActive(true, Color.yellow);
        //        return;
        //    }
        //}
    }
    #endregion

    #region �����
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

        // ���� Ŭ����
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
            obj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f); // ũ�� ����
            obj.transform.SetParent(parentObject.transform); // �θ� ����

        }
    }
    #endregion
}
