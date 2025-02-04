using Google.Protobuf.Protocol;
using Data;
using UnityEngine;

public class Projectile : BaseObject
{
    private ProjectileData _data;
    private Creature _target;
    private Creature _owner;

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

    public void SetInfo(ProjectileInfo projInfo, int targetId)
    {
        int templateId = Utils.GetTemplateIdFromId(projInfo.ObjectInfo.ObjectId);
        Managers.Data.ProjectileDict.TryGetValue(templateId, out _data);

        ObjectId = projInfo.ObjectInfo.ObjectId;
        PosInfo = projInfo.ObjectInfo.PosInfo;
        MoveSpeed = _data.Speed;
        _target = Managers.Object.FindCreatureById(targetId);
        _owner = Managers.Object.FindCreatureById(projInfo.OwnerId);
        if (_owner != null)
            transform.position = _owner.CenterPos;
        //GameObject go = Managers.Object.FindById(targetId);
        //_target = go.GetComponent<Creature>();
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