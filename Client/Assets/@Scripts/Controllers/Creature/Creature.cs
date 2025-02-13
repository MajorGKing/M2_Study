using System;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using Data;
using UnityEngine;

public class Creature : BaseObject
{
    public Dictionary< /*effectId*/int, GameObject> EffectParticles = new Dictionary< /*effectId*/int, GameObject>();
    public Dictionary< /*effectId*/int, EffectData> CurrentEffects = new Dictionary< /*effectId*/int, EffectData>();

    public DamageFontController DamageFontController { get; set; }
    protected UI_ObjectHUD ObjectHUD;
    protected bool _isMonitored = false;

    public virtual bool IsMonitored
    {
        get => _isMonitored;
        set
        {
            _isMonitored = value;
            if (value == true)
                ObjectHUD.SetHpBar();
            else
                ObjectHUD.DeactivateAllHpBars();
        }
    }

    protected CreatureInfo _creatureInfo { get; set; } = new CreatureInfo();

    public StatInfo TotalStat
    {
        get { return _creatureInfo.TotalStatInfo; }
        set { _creatureInfo.TotalStatInfo = value; }
    }

    public float Hp
    {
        get { return TotalStat.Hp; }
        set
        {
            TotalStat.Hp = Math.Clamp(value, 0, TotalStat.MaxHp);
            UpdateHpBar();
        }
    }

    public float Mp
    {
        get { return TotalStat.Mp; }
        set { TotalStat.Mp = Math.Clamp(value, 0, TotalStat.MaxMp); }
    }

    public int TemplateId { get; private set; }

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

    #region LifeCycle

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void Awake()
    {
        base.Awake();
    }

    public virtual void SetInfo(int templateId)
    {
        TemplateId = templateId;
        SetSpineAnimation(SortingLayers.HERO, "SkeletonAnimation");
        ObjectState = EObjectState.Idle;
        SetOutLine();
        DamageFontController = gameObject.GetOrAddComponent<DamageFontController>();
        AddHud();
        IsMonitored = false;

    }

    protected void AddHud()
    {
        if (ObjectHUD == null)
        {
            GameObject obj = Managers.Resource.Instantiate("UI_ObjectHUD", gameObject.transform);
            ObjectHUD = obj.GetComponent<UI_ObjectHUD>();
        }

        ObjectHUD.SetInfo(this);
    }

    protected void SetOutLine()
    {
	    //아웃라인
	    GameObject obj = Utils.FindChild(gameObject, "Outline", true);
	    if (obj != null)
	    {
		    OutLine = obj.AddComponent<OutlineController>();
		    OutLine.SetInfo(this);
	    }
	    else
	    {
		    Debug.LogError($"{gameObject.name} , Outline not found");
	    }
    }

    #endregion

    #region AI

    protected override void UpdateMove()
    {
        base.UpdateMove();

        // 이동 끝났으면.
        if (LerpCellPosCompleted)
        {
            ObjectState = EObjectState.Idle;
            return;
        }
    }

    #endregion

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
        if (ObjectHUD == null)
            return;

        float ratio = 0.0f;
        if (TotalStat.MaxHp > 0)
            ratio = (Hp) / TotalStat.MaxHp;

        ObjectHUD.Refresh(ratio);
    }

    public virtual void OnDamaged()
    {
    }

    public virtual void OnDead()
    {
        ObjectState = EObjectState.Dead;
        IsMonitored = false;
    }

	public virtual void HandleSkillPacket(S_Skill packet)
	{
		if (Managers.Data.SkillDict.TryGetValue(packet.TemplateId, out SkillData skillData) == false)
			return;

        // 1. 스킬 상태로 변경.
        ObjectState = EObjectState.Skill;

		// 2. 타겟 방향 주시, 타겟 히트파티클 스폰 예약
		Creature target = Managers.Object.FindCreatureById(packet.TargetId);
		if (target != null && target != this)
		{
			LookAtTarget(target);
			StartCoroutine(ReserveHitParticles(skillData, target));
		}
		
		// 3. 사운드, 애니메이션 등 실행.
		ExecuteSkillAction(packet.TemplateId);
		
		// 5. 각 스킬의 애니메이션 시간(공속 적용)만큼 대기 한다.
		// TODO 공속 적용 한 delay 구하기
		Spine.Animation animation = SkeletonAnim.skeleton.Data.FindAnimation(skillData.AnimName);
		float delay = animation.Duration;
		StartWait(delay);
	}

	protected IEnumerator ReserveHitParticles(SkillData skillData, BaseObject target)
	{
		if (string.IsNullOrEmpty(skillData.GatherTargetPrefabName) == true)
			yield break;
		
		float delay = 0.0f;
		
		// 1. 발사체 도착시간 계산
		bool isProjectileSkill = skillData.ProjectileId != 0;
		if (isProjectileSkill)
		{
			if (Managers.Data.ProjectileDict.TryGetValue(skillData.ProjectileId, out ProjectileData projectileData))
			{
				float distance = Vector3.Distance(transform.position, target.transform.position);
				delay += distance / projectileData.Speed;
			}
		}
		
		//2. + 스킬 애니메이션 이벤트 시간
		delay += skillData.DelayTime;
		
		yield return new WaitForSeconds(delay);
		
		if (target == null)
			yield break;

        Managers.Object.SpawnParticle(skillData.GatherTargetPrefabName, target.LookLeft, target.transform, true);
    }

	#endregion

    #region Effect
    public void ApplyEffect(S_ApplyEffect packet)
    {
        if (Managers.Data.EffectDict.TryGetValue(packet.EffectTemplateId, out EffectData effectData) == false)
            return;

        CurrentEffects.Add(packet.EffectId, effectData);
        StateFlag = packet.StateFlag; // TEMP

        // 1. 이펙트 파티클스폰.
        if (string.IsNullOrEmpty(effectData.PrefabName) == false)
        {
            // 1-1. 이펙트 파티클 메모리에 저장. 
            ParticleController effect = Managers.Object.SpawnParticle(effectData.PrefabName, LookLeft, transform);
            if (effect != null)
                EffectParticles.Add(packet.EffectId, effect.gameObject);

            // 1-2. 이펙트 파티클 소멸 예약.
            if (effectData.DurationPolicy == EDurationPolicy.Duration)
                StartCoroutine(CoRemoveEffect(packet.EffectId, packet.RemainingTicks));
        }

		// 2. 필요한 경우 데미지폰트 추가
		if (effectData.EffectType == EEffectType.BuffStun)
		{
			DamageFontController.AddDamageFont(0, transform, EFontType.Stun);
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

    //IEnumerator CoSpawnAoE(float seconds, SkillData skillData, Vector3 pos)
    //{
    //    yield return new WaitForSeconds( seconds );
    //    if (string.IsNullOrEmpty(skillData.GatherTargetPrefabName))
    //        yield break;

    //    var pc = Managers.Object.SpawnParticle(skillData.GatherTargetPrefabName);
    //    pc.gameObject.transform.position = pos;
    //}
    #endregion

    #region 사운드 , 애니메이션 등
    protected virtual void ExecuteSkillAction(int skillTemplateId)
    {
        if (Managers.Data.SkillDict.TryGetValue(skillTemplateId, out SkillData skillData) == false)
            return;

        PlayAnimation(0, skillData.AnimName, false);
        AddAnimation(0, AnimName.IDLE, true, 0);

        if (!string.IsNullOrEmpty(skillData.PrefabName))
        {
            Managers.Object.SpawnParticle(skillData.PrefabName, LookLeft, transform, false);
        }
    }
    #endregion

    #region PacketHandler

	public void HandleChangeOneStat(EStatType statType, float value, float diff, EFontType fontType)
	{
        // TEMP
        if (statType == EStatType.Hp)
            Hp = value;
        else if (statType == EStatType.Mp)
            Mp = value;
        
		//TODO 내가 관심있어 하는 몬스터에게만 적용
		if(IsMonitored)
			DamageFontController.AddDamageFont(diff, transform, fontType);
	}

	#endregion
}
