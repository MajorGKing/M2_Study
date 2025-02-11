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

        public bool Boss { get; private set; }
        public Vector2Int SpawnPosition { get; set; }
        public int SpawnRange { get; set; }

        MonsterAIController _ai;

        

        public Monster()
        {
            ObjectType = EGameObjectType.Monster;
            SkillComp = new SkillComponent(this);
        }

        public void Init(int templateId)
        {
            if (DataManager.MonsterDict.TryGetValue(templateId, out MonsterData monsterData) == false)
                return;

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

        public override void OnDead(BaseObject attacker)
        {
            if(attacker.IsValid() == false) 
                return;

            BaseObject owner = attacker.GetOwner();
            if (owner.ObjectType == EGameObjectType.Hero)
            {
                Hero hero = owner as Hero;
                if (hero.Inven.IsInventoryFull() == false)
                {
                    RewardData rewardData = GetRandomReward();
                    if(rewardData != null)
                        DBManager.RewardHero(hero, rewardData);
                }

                // 나머지
                if(MonsterData.DropTable != null)
                    hero.RewardExpAndGold(MonsterData.DropTable);
            }

            _ai.OnDead(attacker);
            base.OnDead(attacker);
        }

        private RewardData GetRandomReward()
        {
            if (MonsterData.DropTable == null)
                return null;
            if (MonsterData.DropTable.Rewards == null)
                return null;
            if (MonsterData.DropTable.Rewards.Count <= 0)
                return null;

            return MonsterData.DropTable.Rewards.RandomElementByWeight(e => e.Probability);
        }

    }
}
