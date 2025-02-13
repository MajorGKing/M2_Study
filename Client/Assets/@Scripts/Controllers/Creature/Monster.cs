using Data;
using Google.Protobuf.Protocol;

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

	public override string GetObjectName()
	{
		//TODO @@다국어
		return MonsterData.NameTextId;
	}
	
	protected override void Update()
	{
		// 기본적으로 모든 물체는 칸 단위로 움직이지만, 클라에서 '스르륵' 움직이는 보정 처리를 해준다.
		UpdateLerpToCellPos(MoveSpeed, true);
	}

	public void InitMonster(CreatureInfo creatureInfo)
	{
		_creatureInfo = creatureInfo;
		ObjectId = creatureInfo.ObjectInfo.ObjectId;
		PosInfo = creatureInfo.ObjectInfo.PosInfo;
		TotalStat = creatureInfo.TotalStatInfo;
		SyncWorldPosWithCellPos();
	}

    public override void SetInfo(int templateId)
    {
        if(Managers.Data.MonsterDict.TryGetValue(templateId, out MonsterData data))
        {
            MonsterData = data;
        }

        ExtraCells = MonsterData.ExtraCells;

        base.SetInfo(templateId);
    }

    #region Battle
    public override bool IsEnemy(BaseObject target)
    {
        if (base.IsEnemy(target) == false)
            return false;

        return target.ObjectType == EGameObjectType.Hero;
    }
    #endregion
}
