using Google.Protobuf.Protocol;
using Scripts.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Creature : BaseObject
{
    public Dictionary< /*templateId*/int, GameObject> CurrentEffects = new Dictionary< /*templateId*/int, GameObject>();
    protected UI_HPBar _hpBar;
    StatInfo _stat = new StatInfo();
    public virtual StatInfo TotalStat
    {
        get { return _stat; }
        set
        {
            if (_stat.Equals(value))
                return;

            _stat.MergeFrom(value);
            UpdateHpBar();
        }
    }

    public int TemplateId { get; private set; }

    public float Hp
    {
        get { return TotalStat.Hp; }
        set
        {
            float diff = TotalStat.Hp = value;
            TotalStat.Hp = value;
            UpdateHpBar();
        }
    }

    #region CreatureFlag
    int _stateFlag = 0;
    public int StateFlag
    {
        get { return _stateFlag; }
        set { _stateFlag = value; }
    }

    bool GetStateFlag(ECreatureStateFlag type)
    {
        return (StateFlag & (1 << (int)type)) != 0;
    }

    public void SetStateFlag(ECreatureStateFlag type, bool value)
    {
        if(value)
        {
            StateFlag |= (1 << (int)type);
        }
        else
        {
            StateFlag &= ~(1 << (int)type);
        }
    }

    public bool IsPoison
    {
        get { return GetStateFlag(ECreatureStateFlag.Poison); }
        set { SetStateFlag(ECreatureStateFlag.Poison, value); }
    }

    public bool IsStunned
    {
        get { return GetStateFlag(ECreatureStateFlag.Poison); }
        set { SetStateFlag(ECreatureStateFlag.Poison, value); }
    }
    #endregion

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

    protected void AddHpBar()
    {
        if (_hpBar == null)
        {
            GameObject obj = Managers.Resource.Instantiate("UI_HPBar", gameObject.transform);
            _hpBar = obj.GetComponent<UI_HPBar>();
        }

        _hpBar.SetInfo(this);
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

    #region Battle

    public virtual void UpdateHpBar()
    {
        if (_hpBar == null)
            return;

        float ratio = 0.0f;
        if (TotalStat.MaxHp > 0)
            ratio = (Hp) / TotalStat.MaxHp;

        _hpBar.Refresh(ratio);
    }

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
            StartCoroutine(CoSpawnAoE(skillData.DelayTime, skillData, aoePos));
        }

        //�� ��ų�� �ִϸ��̼� �ð�(���� ����)��ŭ ��� �Ѵ�.
        float delay = 1;
        var animation = SkeletonAnim.skeleton.Data.FindAnimation(skillData.AnimName);

        //TODO ���� ���� �� delay ���ϱ�
        delay = animation.Duration;

        StartWait(delay);
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
        if (_coWait != null)
            StopCoroutine(_coWait);
        _coWait = null;
    }

    IEnumerator CoSpawnAoE(float seconds, SkillData skillData, Vector3 pos)
    {
        yield return new WaitForSeconds( seconds );
        if (string.IsNullOrEmpty(skillData.GatherTargetPrefabName))
            yield break;

        var pc = Managers.Object.SpawnParticle(skillData.GatherTargetPrefabName);
        pc.gameObject.transform.position = pos;
    }
    #endregion

    #region Packet Handler
    public void UpdateEffects(List<int> effectIds)
    {
        List<int> currentEffects = CurrentEffects.Keys.ToList();

        // ������ �����µ� ���� ���� �ֵ� Spawn ó��
        List<int> added = effectIds.Except(currentEffects).ToList();
        foreach (var effectId in added)
        {
            if (Managers.Data.EffectDic.TryGetValue(effectId, out EffectData data) == false)
                return;

            // ��ø�Ǵ� ����Ʈ�� ����
            if (CurrentEffects.ContainsKey(effectId))
                return;

            // 1. ����Ʈ ����
            if (string.IsNullOrEmpty(data.PrefabName) == false)
            {
                ParticleController effect = Managers.Object.SpawnParticle(data.PrefabName, transform);

                if(effect != null)
                {
                    CurrentEffects.Add(effectId, effect.gameObject);
                }
            }

            // 2. �ʿ��� ��� ��������Ʈ �߰�
            if (data.EffectType == EEffectType.BuffStun)
            {
                Managers.Object.ShowDamageFont(CenterPos, 0, transform, EDamageType.Stun);
            }
            // 3. UI Update
        }

        // ������ �־��µ� ����� �ֵ� Despawn ó��
        List<int> removed = currentEffects.Except(effectIds).ToList();
        foreach (var effectId in removed)
        {
            if (CurrentEffects.TryGetValue(effectId, out GameObject effectObj) == false)
                return;

            Managers.Resource.Destroy(effectObj);
            CurrentEffects.Remove(effectId);
        }
    }
    #endregion
}
