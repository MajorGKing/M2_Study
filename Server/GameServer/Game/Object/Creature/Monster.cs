using GameServer.Game;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game;
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

        MonsterAIController _ai;

        public bool Boss { get; private set; }

        public Monster()
        {
            ObjectType = EGameObjectType.Monster;
            SkillComp = new SkillComponent(this);
        }

        public void Init(int templateId)
        {
            if (DataManager.MonsterDict.TryGetValue(templateId, out MonsterData monsterData) == false)
            {
                Console.WriteLine($"No Data in db {templateId}");
                return;
            }

            TemplateId = templateId;
            _ai = new MonsterAIController(this);

            MonsterData = monsterData;
            BaseStat.MergeFrom(monsterData.Stat);
            BaseStat.Hp = BaseStat.MaxHp;
            CreatureInfo.TotalStatInfo = BaseStat;
            TotalStat.MergeFrom(BaseStat);
            CreatureInfo.TotalStatInfo.MergeFrom(TotalStat);

            State = EObjectState.Idle;
            Boss = monsterData.IsBoss;
            ExtraCells = monsterData.ExtraCells;

            foreach (var skillData in monsterData.SkillMap.Values)
            {
                Console.WriteLine($"{Data.Name} add Skill : {skillData.Name} , {skillData.EffectData}");
                SkillComp.RegisterSkill(skillData.TemplateId);
            }
        }

        public override bool IsEnemy(BaseObject target)
        {
            if (base.IsEnemy(target) == false)
                return false;

            if (target.ObjectType == EGameObjectType.Hero)
                return true;

            return false;
        }

        public override void Update()
        {
            _ai.Update();
        }

        public override float OnDamaged(BaseObject attacker, float damage)
        {
            float ret = base.OnDamaged(attacker, damage);

            // 비선공몹은 데미지를 입으면 반격함
            if (MonsterData.IsAggressive == false)
            {
                _ai.OnDamaged(attacker, damage);
            }

            return ret;
        }

    }
}
