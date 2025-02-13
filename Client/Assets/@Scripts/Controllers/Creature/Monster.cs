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
		//TODO @@�ٱ���
		return MonsterData.NameTextId;
	}
	
	protected override void Update()
	{
		// �⺻������ ��� ��ü�� ĭ ������ ����������, Ŭ�󿡼� '������' �����̴� ���� ó���� ���ش�.
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
