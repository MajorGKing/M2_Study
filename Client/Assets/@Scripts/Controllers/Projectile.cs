using System;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using Scripts.Data;
using UnityEngine;

public class Projectile : BaseObject
{
    private ProjectileData _data;
    private Creature _target;

    public override PositionInfo PosInfo
    {
        get { return _positionInfo; }
        set
        {
            _positionInfo = value;

            Vector3Int cellPos = new Vector3Int(value.PosX, value.PosY, 0);
            SetCellPos(cellPos, forceMove: true);
        }
    }

    protected override void Awake()
    {
        base.Awake();

        ObjectType = EGameObjectType.Projectile;
        ObjectState = EObjectState.Move;
    }

    public void SetInfo(int templateId, int targetId)
    {
        Managers.Data.ProjectileDic.TryGetValue(templateId, out _data);
        MoveSpeed = _data.ProjSpeed;

        GameObject go = Managers.Object.FindById(targetId);
        _target = go.GetComponent<Creature>();
    }

    protected override void UpdateAnimation()
    {

    }

    protected override void Update()
    {
        if (_target == null || MoveSpeed == 0)
        {
            Managers.Object.Despawn(ObjectId);
            return;
        }

        Vector3 destPos = _target.transform.position;
        Vector3 dir = destPos - transform.position;

        if (dir.x < 0)
            LookLeft = true;
        else if (dir.x > 0)
            LookLeft = false;

        transform.rotation = LookAt2D(dir);

        float moveDist = MoveSpeed * Time.deltaTime;
        if (dir.magnitude < moveDist)
        {
            transform.position = destPos;
            Managers.Object.Despawn(ObjectId);
            return;
        }

        transform.position += dir.normalized * moveDist;
    }

    protected Quaternion LookAt2D(Vector2 forward)
    {
        return Quaternion.Euler(0, 0, Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg);
    }
}