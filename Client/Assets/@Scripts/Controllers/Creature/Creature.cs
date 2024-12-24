using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : BaseObject
{
    StatInfo _stat = new StatInfo();
    public virtual StatInfo TotalStat
    {
        get { return _stat; }
        set
        {
            if (_stat.Equals(value))
                return;

            _stat.MergeFrom(value);
        }
    }

    public int TemplateId { get; private set; }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {

    }

    public virtual void SetInfo(int templateId)
    {
        TemplateId = templateId;
        SetSpineAnimation(SortingLayers.HERO, "SkeletonAnimation");
    }

    protected override void UpdateMove()
    {
        base.UpdateMove();

        // �̵� ��������.
        if (LerpCellPosCompleted)
        {
            ObjectState = EObjectState.Idle;
            return;
        }
    }

    #region Map

    public EFindPathResult FindPathToCellPos(Vector3 destWorldPos, int maxDepth, out List<Vector3Int> path, bool forceMoveCloser = false)
    {
        Vector3Int destCellPos = Managers.Map.World2Cell(destWorldPos);
        return FindPathToCellPos(destCellPos, maxDepth, out path, forceMoveCloser);
    }

    public EFindPathResult FindPathToCellPos(Vector3Int destCellPos, int maxDepth, out List<Vector3Int> path, bool forceMoveCloser = false)
    {
        path = new List<Vector3Int>();

        if (CellPos == destCellPos)
            return EFindPathResult.FailSamePosition;

        if (LerpCellPosCompleted == false)
            return EFindPathResult.FailLerpcell;

        // A*
        path = Managers.Map.FindPath(this, CellPos, destCellPos, maxDepth);
        if (path.Count < 2)
            return EFindPathResult.FailNoPath;

        if (forceMoveCloser)
        {
            Vector3Int diff1 = CellPos - destCellPos;
            Vector3Int diff2 = path[1] - destCellPos;
            if (diff1.sqrMagnitude <= diff2.sqrMagnitude)
                return EFindPathResult.FailNoPath;
        }

        Vector3Int dirCellPos = path[1] - CellPos;
        Vector3Int nextPos = CellPos + dirCellPos;

        //if (Managers.Map.MoveTo(this, nextPos) == false)
        //	return EFindPathResult.FailMoveTo;

        return EFindPathResult.Success;
    }

    public bool MoveToCellPos(Vector3Int destCellPos, int maxDepth = 2, bool forceMoveCloser = false)
    {
        if (LerpCellPosCompleted == false)
            return false;

        return Managers.Map.MoveTo(this, destCellPos);
    }

    #endregion

    #region Wait
    protected Coroutine _coWait;
    protected void StartWait(float seconds)
    {
        CancelWait();
        _coWait = StartCoroutine(CoWait(seconds));
    }

    IEnumerator CoWait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _coWait = null;
    }

    protected void CancelWait()
    {
        if(_coWait != null )
            StopCoroutine(_coWait);
        _coWait = null;
    }

    #endregion

}