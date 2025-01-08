using System;
using System.Collections;
using Google.Protobuf.Protocol;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Rendering;
using Event = Spine.Event;

public class BaseObject : MonoBehaviour
{
    public int ObjectId { get; set; }
    public EGameObjectType ObjectType { get; protected set; } = EGameObjectType.None;
    public SkeletonAnimation SkeletonAnim { get; set; }
    public float UpdateAITick { get; protected set; }
    private GameObject CenterObject { get; set; }

    public Vector3 CenterPos
    {
        get
        {
            if (CenterObject == null)
                return transform.position;
            else
                return transform.position + CenterObject.transform.localPosition;
        }
    }

    public int ExtraCells = 0;
    // TEMP
    public virtual float MoveSpeed { get; set; } = 5f;

    bool _lookLeft = true;
    public bool LookLeft
    {
        get { return _lookLeft; }
        set
        {
            _lookLeft = value;
            Flip(!value);
        }
    }

    protected PositionInfo _positionInfo = new PositionInfo();
    public virtual PositionInfo PosInfo
    {
        get { return _positionInfo; }
        set
        {
            if (_positionInfo.Equals(value))
                return;

            var cellPos = new Vector3Int(value.PosX, value.PosY, 0);
            MoveDir = value.MoveDir;

            Managers.Map.MoveTo(this, cellPos);

            // 내 플레이어는 상태 덮어쓰지 않고, 알아서 관리한다.
            bool isMyHero = this is MyHero;
            if (isMyHero == false)
            {
                ObjectState = value.State;
            }
        }
    }

    [SerializeField]
    protected EObjectState _objectState = EObjectState.None;
    public virtual EObjectState ObjectState
    {
        get { return PosInfo.State; }
        set
        {
            if (_objectState == value)
                return;

            _objectState = value;
            PosInfo.State = value;
            UpdateAnimation();
        }
    }

    [SerializeField]
    protected EMoveDir _moveDir = EMoveDir.None;
    public EMoveDir MoveDir
    {
        get { return PosInfo.MoveDir; }
        set
        {
            if (_moveDir == value)
                return;

            _moveDir = value;
            PosInfo.MoveDir = value;
        }
    }

    protected virtual void Awake()
    {
        CenterObject = Utils.FindChild(gameObject, "CenterPosition");
    }

    protected virtual void OnEnable()
    {
        StartCoroutine(CoUpdateAI());

    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {

    }

    #region AI (FSM)
    protected virtual IEnumerator CoUpdateAI()
    {
        while (true)
        {
            switch (ObjectState)
            {
                case EObjectState.Idle:
                    UpdateIdle();
                    UpdateAITick = 0.0f;
                    break;
                case EObjectState.Move:
                    UpdateAITick = 0.0f;
                    UpdateMove();
                    break;
                case EObjectState.Skill:
                    UpdateAITick = 0.1f;
                    UpdateSkill();
                    break;
                case EObjectState.Dead:
                    UpdateAITick = 1f;
                    UpdateDead();
                    break;
            }

            if (UpdateAITick > 0)
                yield return new WaitForSeconds(UpdateAITick);
            else
                yield return null;
        }
    }

    protected virtual void UpdateIdle() { }
    protected virtual void UpdateMove() { }
    protected virtual void UpdateSkill() { }
    protected virtual void UpdateDead() { }
    #endregion

    #region Animation
    protected virtual void UpdateAnimation()
    {
        switch (ObjectState)
        {
            case EObjectState.Idle:
                PlayAnimation(0, AnimName.IDLE, true);
                break;
            case EObjectState.Skill:
                break;
            case EObjectState.Move:
                PlayAnimation(0, AnimName.MOVE, true);
                break;
            case EObjectState.Dead:
                PlayAnimation(0, AnimName.DEAD, false);
                break;
        }
    }

    protected void SetSpineAnimation(int sortingOrder, string objName)
    {
        SkeletonAnim = GetComponent<SkeletonAnimation>();
        if (SkeletonAnim == null)
            SkeletonAnim = Utils.FindChild<SkeletonAnimation>(gameObject, objName);

        //SkeletonAnim.Initialize(true);
        RegisterAnimEvent();

        SortingGroup sg = Utils.GetOrAddComponent<SortingGroup>(SkeletonAnim.gameObject);
        sg.sortingOrder = sortingOrder;
    }

    protected void ClearSpineAnimation()
    {
        SkeletonAnim.skeletonDataAsset = Managers.Resource.Load<SkeletonDataAsset>("empty_skeleton");
        SkeletonAnim.clearStateOnDisable = true;
        SkeletonAnim.Initialize(true);
    }

    protected void RegisterAnimEvent()
    {
        if (SkeletonAnim.AnimationState != null)
        {
            SkeletonAnim.AnimationState.Event += OnAnimEventHandler;
            SkeletonAnim.AnimationState.Complete += OnAnimCompleteHandler;
        }
    }

    public TrackEntry PlayAnimation(int trackIndex, string animName, bool loop, bool mixDuration = true)
    {
        if (SkeletonAnim == null)
            return null;
        if (SkeletonAnim.AnimationState == null)
            return null;

        TrackEntry entry = SkeletonAnim.AnimationState.SetAnimation(trackIndex, animName, loop);

        if (mixDuration == false)
            entry.MixDuration = 0;
        else
        {
            if (animName == AnimName.DEAD || animName.Contains("skill_a") || animName.Contains("skill_b"))
                entry.MixDuration = 0;
            else
                entry.MixDuration = 0.2f;
        }

        return entry;
    }

    public TrackEntry GetCurrentAnimation()
    {
        var trackEntry = SkeletonAnim.state.GetCurrent(0);
        return trackEntry;
    }

    public TrackEntry AddAnimation(int trackIndex, string AnimName, bool loop, float delay)
    {
        return SkeletonAnim.AnimationState.AddAnimation(trackIndex, AnimName, loop, delay);
    }

    public float GetSpineHeight()
    {
        float x, y, width, height;
        float[] vertexBuffer = null;
        SkeletonAnim.skeleton.GetBounds(out x, out y, out width, out height, ref vertexBuffer);
        Debug.Log($"GetSpineHeight {height}");
        return height;
    }

    public void Flip(bool flag)
    {
        if (SkeletonAnim == null)
            return;

        SkeletonAnim.Skeleton.ScaleX = flag ? -1 : 1;
    }

    protected virtual void OnAnimEventHandler(TrackEntry trackEntry, Event e) { }
    protected virtual void OnAnimCompleteHandler(TrackEntry arg1) { }

    protected virtual void OnDisable()
    {
        if (SkeletonAnim == null)
            return;
        if (SkeletonAnim.AnimationState == null)
            return;

        SkeletonAnim.AnimationState.Event -= OnAnimEventHandler;
        SkeletonAnim.AnimationState.Complete -= OnAnimCompleteHandler;
    }

    #endregion

    #region Map
    public bool LerpCellPosCompleted { get; protected set; }

    [SerializeField] Vector3Int _cellPos;
    public Vector3Int CellPos
    {
        get { return _cellPos; }
        protected set
        {
            _cellPos = value;
            LerpCellPosCompleted = false;
        }
    }

    public void SetCellPos(Vector3Int cellPos, bool forceMove = false)
    {
        CellPos = cellPos;
        LerpCellPosCompleted = false;

        if (forceMove)
        {
            transform.position = Managers.Map.Cell2World(CellPos);
            LerpCellPosCompleted = true;
        }
    }

    public void UpdateLerpToCellPos(float moveSpeed, bool canFlip = true)
    {
        if (LerpCellPosCompleted)
            return;

        Vector3 destPos = Managers.Map.Cell2World(CellPos);
        Vector3 dir = destPos - transform.position;
        if (canFlip)
        {
            if (dir.x < 0)
                LookLeft = true;
            else if (dir.x > 0)
                LookLeft = false;
        }

        float moveDist = moveSpeed * Time.deltaTime;
        if (dir.magnitude < moveDist)
        {
            // 다 이동 했으면 맵 그리드 갱신
            SyncWorldPosWithCellPos();
            transform.position = destPos;
            LerpCellPosCompleted = true;
            return;
        }

        transform.position += dir.normalized * moveDist;
    }

    public void SyncWorldPosWithCellPos()
    {
        Managers.Map.MoveTo(this, CellPos, forceMove: true);
    }
    #endregion

    #region Helper
    public void LookAtTarget(GameObject target)
    {
        if (target == null)
            return;
        Vector2 dir = target.transform.position - transform.position;
        LookAtTarget(dir);
    }

    public void LookAtTarget(BaseObject target)
    {
        if (target == null)
            return;
        Vector2 dir = target.transform.position - transform.position;
        LookAtTarget(dir);
    }
    
    public void LookAtTarget(Vector3 dir)
    {
        if (dir.x < 0)
            LookLeft = true;
        else if (dir.x > 0)
            LookLeft = false;
    }
    #endregion
}

