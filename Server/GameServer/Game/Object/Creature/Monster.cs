using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
	public class Monster : Creature
	{
		public MonsterData MonsterData { get; set; }
        public override CreatureData Data
        {
            get { return MonsterData; }
        }

        public bool Boss { get; private set; }

        public Monster()
        {
            ObjectType = EGameObjectType.Monster;
        }

        public void Init(int templateId)
        {
            if (DataManager.MonsterDict.TryGetValue(templateId, out MonsterData monsterData) == false)
            {
                Console.WriteLine($"No Data in db {templateId}");
                return;
            }

            TemplateId = templateId;

            MonsterData = monsterData;
            BaseStat.MergeFrom(monsterData.Stat.StatInfo);
            BaseStat.Hp = BaseStat.MaxHp;
            TotalStat.MergeFrom(BaseStat);

            State = EObjectState.Idle;
            Boss = monsterData.IsBoss;
            ExtraCells = monsterData.ExtraCells;

            
        }
    }
}
