using GameServer.Game;
using Google.Protobuf.Protocol;
using Server.Data;

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

        public MonsterAIController AI { get; private set; }
        public AggroComponent Aggro { get; private set; }



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
            AI = new MonsterAIController(this);
            Aggro = new AggroComponent();

            MonsterData = monsterData;

			StatComp.BaseStat.MergeFrom(monsterData.Stat);
			StatComp.BaseStat.Hp = StatComp.BaseStat.MaxHp;

			StatComp.TotalStat.MergeFrom(StatComp.BaseStat);
			CreatureInfo.TotalStatInfo = StatComp.TotalStat;

            State = EObjectState.Idle;
            Boss = monsterData.IsBoss;
            ExtraCells = monsterData.ExtraCells;

            foreach (var skillData in monsterData.SkillMap.Values)
            {
                //Console.WriteLine($"{Data.Name} add Skill : {skillData.Name} , {skillData.EffectData}");
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
            base.Update();

            AI.Update();
        }

        public override bool OnDamaged(BaseObject attacker, float damage)
        {
            if (Room == null)
                return false;

            if (State == EObjectState.Dead)
                return false;

            // 1. 어그로 매니저에 전달.
            if (attacker.ObjectType == EGameObjectType.Hero)
                Aggro.OnDamaged(attacker.ObjectId, damage);

            // 2. AI 매니저에 전달.
            AI.OnDamaged(attacker, damage);

            return base.OnDamaged(attacker, damage);
        }

        public override void OnDead(BaseObject attacker)
        {
            // 1. 어그로 수치가 가장 높고, 같은 방에 있는 영웅한테 준다.
            GiveRewardToTopAttacker();

            // 2. 모든 공격자들에게 킬 이벤트 준다(리니지랑 같은방식)
            BroadcastKillEventToAllAttackers();

            // 3. AI 매니저에 전달.
            AI.OnDead(attacker);

            base.OnDead(attacker);
        }

        private void GiveRewardToTopAttacker()
        {
            // 어그로 수치가 가장 높고, 같은 방에 있는 영웅한테 준다.
            List<int> sortedAttackerIds = Aggro.GetTopAttackers();
            foreach (int attackerId in sortedAttackerIds)
            {
                Hero hero = Room.GetHeroById(attackerId);
                if (hero != null)
                {
                    GiveReward(hero);
                    return;
                }
            }
        }

        private void GiveReward(Hero hero)
        {
            if(hero.Inven.IsInventoryFull() == false)
            {
                // 퀘스트 아이템 드롭, 일반 아이템 드롭 따로 적용
                // 1. 퀘스트 아이템을 제외한 Reward
                RewardData rewardData = GetRandomRewardFromMonsterData(isQuestReward: false);
                if (rewardData != null)
                    DBManager.RewardHero(hero, rewardData);

                // 2. Hero가 퀘스트중이고, 몬스터 드롭 테이블에 퀘스트 아이템이 있으면.
                List<RewardData> questRewards = hero.QuestComp.FilterNeededItemToProceedQuestTask(MonsterData.RewardTable.Rewards);
                if (questRewards.Count > 0)
                {
                    RewardData questRewardData = questRewards.RandomElementByProbability();
                    if (questRewardData != null)
                        DBManager.RewardHero(hero, questRewardData);
                }
            }

            // 나머지
            if (MonsterData.RewardTable != null)
                hero.RewardExpAndGold(MonsterData.RewardTable);
        }

        private void BroadcastKillEventToAllAttackers()
        {
            List<int> attackerIds = Aggro.GetAllAttackers();
            foreach (int attackerId in attackerIds)
            {
                Hero hero = Room.GetHeroById(attackerId);
                if (hero == null)
                    continue;
                    
                hero.BroadcastEvent(EBroadcastEventType.KillTarget, TemplateId, 1);
            }
        }

        public override void Reset()
        {
            base.Reset();
            AI.Reset();
            Aggro.Reset();
        }

        #region Helpers

        private RewardData GetRandomRewardFromMonsterData(bool isQuestReward = false)
        {
            if (isQuestReward)
                return GetRandomRewardFromMonsterData(r => r.Item.Type == EItemType.Collectible);
            else
                return GetRandomRewardFromMonsterData(r => r.Item.Type != EItemType.Collectible);
        }

        private RewardData GetRandomRewardFromMonsterData(Func<RewardData, bool> condition)
        {
            if (MonsterData.RewardTable == null)
                return null;
            if (MonsterData.RewardTable.Rewards == null)
                return null;
            if (MonsterData.RewardTable.Rewards.Count <= 0)
                return null;

            var filteredRewards = MonsterData.RewardTable.Rewards
                                  .Where(condition)
                                  .ToList();

            return filteredRewards.RandomElementByProbability();
        }

        #endregion
    }
}
