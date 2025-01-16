using Google.Protobuf.Protocol;
using Scripts.Data.SO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : Creature
{
    //Temp
    public override float MoveSpeed => MonsterData.Stat.MoveSpeed;
    public MonsterData MonsterData { get; set; }

    public bool IsBoss
    {
        get
        {
            if(MonsterData != null)
                return MonsterData.IsBoss;
            else 
                return false;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        ObjectType = EGameObjectType.Monster;
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        // �⺻������ ��� ��ü�� ĭ ������ ����������, Ŭ�󿡼� '������' �����̴� ���� ó���� ���ش�.
        UpdateLerpToCellPos(MoveSpeed, true);
    }

    public void InitMonster(CreatureInfo creatureInfo)
    {
        ObjectId = creatureInfo.ObjectInfo.ObjectId;
        PosInfo = creatureInfo.ObjectInfo.PosInfo;
        TotalStat = creatureInfo.TotalStatInfo;
        SyncWorldPosWithCellPos();
    }

    public override void SetInfo(int templateId)
    {
        base.SetInfo(templateId);
        if(Managers.Data.MonsterDic.TryGetValue(templateId, out MonsterData data))
        {
            MonsterData = data;
        }
    }
}
