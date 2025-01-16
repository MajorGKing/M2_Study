using Google.Protobuf.Protocol;
using Scripts.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Creature : BaseObject
{
    public Dictionary< /*effectId*/int, GameObject> EffectParticles = new Dictionary< /*effectId*/int, GameObject>();
    public Dictionary< /*effectId*/int, EffectData> CurrentEffects = new Dictionary< /*effectId*/int, EffectData>();

    protected UI_HPBar _hpBar;

    protected CreatureInfo _creatureInfo { get; set; } = new CreatureInfo();

    public StatInfo TotalStat
    {
        get { return _creatureInfo.TotalStatInfo; }
        set { _creatureInfo.TotalStatInfo = value; }
    }

    public int TemplateId { get; private set; }

    public float Hp
    {
        get { return TotalStat.Hp; }
        set
        {
            if(ObjectType == EGameObjectType.Monster)
            {
                Debug.Log($"{name} : { TotalStat.Hp }");
            }
            TotalStat.Hp = Math.Clamp(value, 0, TotalStat.MaxHp);
            UpdateHpBar();
        }
    }

    public float Mp
    {
        get { return TotalStat.Mp; }
        set { TotalStat.Mp = Math.Clamp(value, 0, TotalStat.MaxMp); }
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
    public virtual bool IsEnemy(BaseObject target)
    {
        if (target == null)
            return false;
        if(target == this)
            return false;

        return true;
    }

    public bool IsFriend(BaseObject target)
    {
        return IsEnemy(target) == false;
    }

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
        if (Managers.Data.SkillDic.TryGetValue(packet.TemplateId, out SkillData skillData) == false)
            return;

        // 1. ��ų ���·� ����.
        ObjectState = EObjectState.Skill;

        // 2. Ÿ�� ���� �ֽ�.
        GameObject target = Managers.Object.FindById(packet.TargetId);
        if (target != null && target != this)
            LookAtTarget(target);

        // 3. ����, �ִϸ��̼� �� ����.
        PlayAnimation(0, skillData.AnimName, false);
        AddAnimation(0, AnimName.IDLE, true, 0);

        // 4. ��ų ����Ʈ
        if (string.IsNullOrEmpty(skillData.PrefabName) == false)
        {
            ParticleController pc = Managers.Object.SpawnParticle(skillData.PrefabName, LookLeft, transform);
        }

        // 5. �� ��ų�� �ִϸ��̼� �ð�(���� ����)��ŭ ��� �Ѵ�.
        // TODO ���� ���� �� delay ���ϱ�
        Spine.Animation animation = SkeletonAnim.skeleton.Data.FindAnimation(skillData.AnimName);
        float delay = animation.Duration;
        StartWait(delay);
    }

    public virtual void OnDamaged()
    {
    }

    public virtual void OnDead()
    {
        ObjectState = EObjectState.Dead;
    }
    #endregion

    #region Effect
    public void ApplyEffect(S_ApplyEffect packet)
    {
        if (Managers.Data.EffectDic.TryGetValue(packet.EffectTemplateId, out EffectData effectData) == false)
            return;

        CurrentEffects.Add(packet.EffectId, effectData);
        StateFlag = packet.StateFlag; // TEMP

        // 1. ����Ʈ ��ƼŬ����.
        if (string.IsNullOrEmpty(effectData.PrefabName) == false)
        {
            // 1-1. ����Ʈ ��ƼŬ �޸𸮿� ����. 
            ParticleController effect = Managers.Object.SpawnParticle(effectData.PrefabName, LookLeft, transform);
            if (effect != null)
                EffectParticles.Add(packet.EffectId, effect.gameObject);

            // 1-2. ����Ʈ ��ƼŬ �Ҹ� ����.
            if (effectData.DurationPolicy == EDurationPolicy.Duration)
                StartCoroutine(CoRemoveEffect(packet.EffectId, packet.RemainingTicks));
        }
    }

    IEnumerator CoRemoveEffect(int effectId, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        RemoveEffect(effectId);
    }

    public void RemoveEffect(S_RemoveEffect packet)
    {
        RemoveEffect(packet.EffectId);
        StateFlag = packet.StateFlag; // TEMP
    }

    public void RemoveEffect(int effectId)
    {
        CurrentEffects.Remove(effectId);

        if (EffectParticles.TryGetValue(effectId, out GameObject effectObj) == false)
            return;

        Managers.Resource.Destroy(effectObj);
        EffectParticles.Remove(effectId);
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
}
