using System.Collections;
using System.Collections.Generic;
using Data;
using Google.Protobuf.Protocol;
using Scripts.Data;
using Scripts.Data.SO;
using Spine;
using UnityEngine;

public class Hero : Creature
{
    protected HeroData _heroData { get; private set; }
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        ObjectType = EGameObjectType.Hero;
    }

    protected override void Start()
    {
        base.Start();
    }

    // TEMP : 강의용 직관적인 코드. 겹치는 부분 상속 구조로 올려버릴 예정.
    protected override void Update()
    {
        // 기본적으로 모든 물체는 칸 단위로 움직이지만, 클라에서 '스르륵' 움직이는 보정 처리를 해준다.
        UpdateLerpToCellPos(MoveSpeed, true);
    }

    protected override void UpdateMove()
    {
        base.UpdateMove();
    }

    public virtual void SetInfo(int templateId)
    {
        base.SetInfo(templateId);
        if (Managers.Data.HeroDic.TryGetValue(templateId, out HeroData heroData))
        {
            _heroData = heroData;
        }
    }
}
