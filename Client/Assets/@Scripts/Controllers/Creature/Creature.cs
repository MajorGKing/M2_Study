using Google.Protobuf.Protocol;
using Scripts.Data;
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

    #region Battle
    public virtual void HandleSkillPacket(S_Skill packet)
    {
        // ��ų ������ ã�Ƴ���
        if (Managers.Data.SkillDic.TryGetValue(packet.SkillId, out SkillData skillData) == false)
            return;

        ObjectState = EObjectState.Skill;

        Vector3 aoePos = transform.position;

        // Ÿ�ٿ� �����ϴ� ������ ���� ����
        if (packet.OptionalContext != null)
        {
            GameObject target = Managers.Object.FindById(packet.OptionalContext.TargetId);
            if(target != null )
            {
                LookAtTarget(target);
                // aoe ��Ÿ�� ���� �����ֱ�
                aoePos = target.transform.position;
            }
        }

        // ��ų �����ֱ�
        // TODO SkillSound
        PlayAnimation(0, skillData.AnimName, false);
        AddAnimation(0, AnimName.IDLE, true, 0);

        // ��ų ����Ʈ
        if(string.IsNullOrEmpty(skillData.PrefabName) == false)
        {
            // ��ų ����Ʈ�� ������ ��Ÿ���� ��
            ParticleController pc = Managers.Object.SpawnParticle(skillData.PrefabName, transform);

            if (LookLeft)
            {
                pc.transform.Rotate(0, 180, 0);
            }
            else
            {
                pc.transform.Rotate(0, 0, 0);
            }
        }

        //AoE����
        //foreach (var time in skillData.EventTimes)
        {
            //ILHAK START
        }
    }
    #endregion
}