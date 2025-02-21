using Google.Protobuf.Protocol;
using Server.Data;

namespace GameServer.Game
{
    // QuestComponent
    // - Quest
    // -- Task
    // --- Objective
    public class QuestComponent : IBroadcastEventListener
    {
        public Dictionary</*TemplateId*/int, Quest> AllQuests = new Dictionary<int, Quest>();
        public Hero Owner { get; private set; }

        public QuestComponent(Hero owner)
        {
            Owner = owner;
        }

        public void Init(HeroDb heroDb)
        {
            foreach (QuestDb quest in heroDb.Quests)
                AddQuestFromDb(quest);

            if (heroDb.Quests.Count == 0)
                CheckAvailableQuests();
        }

        public void OnBroadcastEvent(EBroadcastEventType type, int targetId, int count)
        {
            // 1. 조건 변화에 의한 퀘스트 추가 확인
            switch(type)
            {
                case EBroadcastEventType.LevelUp:
                    CheckAvailableQuests();
                    break;
                case EBroadcastEventType.CompleteQuest:
                    CheckAvailableQuests();
                    break;
            }

            // 2. 이벤트 전파를 통해 퀘스트 진행 확인.
            foreach(Quest quest in AllQuests.Values.ToList())
            {
                if (quest.State != EQuestState.Processing)
                    continue;

                quest.OnBroadcastEvent(type, targetId, count);
            }
        }

        public void CheckAvailableQuests()
        {
            // 조건에 맞는 QuestData 찾기 (TODO 각종 조건 추가) 
            List<QuestData> availableQuests = new List<QuestData>();

            foreach(QuestData questData in DataManager.QuestDict.Values)
            {
                // 레벨 제한 확인.
                if (Owner.HeroInfoComp.Level < questData.Level)
                    continue;

                // 선행 퀘스트 확인.
                if (questData.RequiredQuestId != 0)
                {
                    if (AllQuests.TryGetValue(questData.RequiredQuestId, out Quest quest) == false)
                        continue;
                    if(quest.State != EQuestState.Completed && quest.State != EQuestState.Rewarded)
                        continue;
                }

                // TODO : 기타 조건 확인 (아이템 보유 수량 등)

                availableQuests.Add(questData);
            }

            foreach(QuestData questData in availableQuests)
            {
                // 이미 있으면 스킵.
                if (AllQuests.TryGetValue(questData.TemplateId, out Quest quest))
                    continue;

                // 퀘스트 추가
                DBManager.AddQuestNoti(Owner, questData);
            }
        }



        #region Helper
        public Quest AddQuestFromDb(QuestDb questDb, bool sendToClient = false)
        {
            Quest quest = Quest.MakeQuest(questDb, Owner);
            if (quest == null)
                return null;

            AllQuests.Add(quest.TemplateId, quest);

            if(sendToClient)
            {
                S_AddOrUpdateQuest pkt = new S_AddOrUpdateQuest();
                pkt.QuestInfo = quest.Info;
                Owner.Session?.Send(pkt);
            }

            return quest;
        }

        public void Clear()
        {
            AllQuests.Clear();
        }

        public List<RewardData> FilterNeededItemToProceedQuestTask(List<RewardData> rewards)
        {
            List<RewardData> questRewards = new List<RewardData>();

            foreach(Quest quest in GetAllProcessingQuests())
            {
                QuestTask questTask = quest.CurrentTask;
                if (questTask == null)
                    continue;

                if(questTask.TaskData.TaskType != EQuestTaskType.CollectItem)
                    continue;

                foreach(RewardData reward in rewards)
                {
                    if(questTask.IsNeededItemToProceedQuestTask(reward.ItemTemplateId))
                        questRewards.Add(reward);
                }
            }

            return questRewards;
        }

        public Quest GetQuestByTemplateId(int templateId)
        {
            AllQuests.TryGetValue(templateId, out Quest quest);
            return quest;
        }

        public Quest GetQuestByCondition(Func<Quest, bool> condition)
        {
            return AllQuests.Values.FirstOrDefault(condition);
        }

        public List<Quest> GetAllQuestsByCondition(Func<Quest, bool> condition)
        {
            return AllQuests.Values.Where(condition).ToList();
        }

        public QuestTask GetCurrentMainQuestTask()
        {
            Quest quest = GetQuestByCondition(q => q.State == EQuestState.Processing && q.QuestData.Type == EQuestType.Main && q.CurrentTask != null);
            return quest?.CurrentTask;
        }


        public List<Quest> GetAllProcessingQuests()
        {
            return GetAllQuestsByCondition(q => q.State == EQuestState.Processing);
        }

        public List<QuestInfo> GetAllQuestInfos()
        {
            return AllQuests.Values.Select(i => i.Info).ToList();
        }
        #endregion
    }
}
