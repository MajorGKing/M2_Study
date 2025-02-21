using Google.Protobuf.Protocol;
using Server.Data;

namespace GameServer.Game
{
    public class Quest : IBroadcastEventListener
    {
        public List<QuestTask> QuestTasks { get; private set; } = new List<QuestTask>();

        public QuestInfo Info { get; } = new QuestInfo();
        public QuestData QuestData { get; private set; }
        public Hero Owner { get; set; }

        private QuestTask _questTask;
        public QuestTask CurrentTask
        {
            get { return _questTask; }
            set
            {
                PrevTask = _questTask;
                _questTask = value;
            }
        }

        public QuestTask PrevTask { get; set; }

        public Quest(QuestDb questDb, Hero owner)
        {
            if (DataManager.QuestDict.TryGetValue(questDb.TemplateId, out QuestData questData) == false)
                return;
            if (questDb.QuestTasks.Count != questData.TaskIds.Count)
                return;

            // 기본 정보 설정.
            QuestData = questData;
            Owner = owner;

            // 패킷 전송용 정보 설정.
            Info.TemplateId = questDb.TemplateId;
            Info.QuestState = questDb.State;
            Info.TaskInfos.Clear();

            // 각 퀘스트 태스크를 순회하며 TaskInfo를 생성.
            foreach (QuestTaskDb taskDb in questDb.QuestTasks)
            {
                // QuestTasks에 추가.
                int index = QuestTasks.Count;
                int templateId = QuestData.TaskIds[index];
                QuestTask questTask = new QuestTask(templateId, taskDb, this);
                QuestTasks.Add(questTask);

                // TaskInfo에 추가.
                Info.TaskInfos.Add(questTask.TaskInfo);
            }

            SetCurrentProcessingTask();
        }

        public void OnBroadcastEvent(EBroadcastEventType type, int targetId, int count)
        {
            // 0. 이미 보상 받았으면 끝.
            if (State != EQuestState.Processing)
                return;

            // 1. 이벤트 전파를 통해 퀘스트 진행 확인.
            foreach (QuestTask task in QuestTasks)
                task.OnBroadcastEvent(type, targetId, count);

            // 2. 바뀐게 없으면 아무 것도 안 함.
            if (CheckAndResetTaskDirtyFlag() == false)
                return;

            // 3. 퀘스트 클리어 확인.
            TryUpdateQuestState();

            // 4. 퀘스트 클리어라면 보상 지급.
            if (State == EQuestState.Completed)
                GiveReward(); // State = EQuestState.Rewarded;

            // 5. DB 저장
            DBManager.SaveQuestNoti(Owner, Info);

            // 6. 패킷 전송
            S_AddOrUpdateQuest pkt = new S_AddOrUpdateQuest();
            pkt.QuestInfo = Info;
            Owner.Session?.Send(pkt);

            // 7. 진행중인 태스크 변경.
            SetCurrentProcessingTask();
        }

        public void TryUpdateQuestState()
        {
            // 완료했거나 이미 보상 받았으면 끝.
            if (State == EQuestState.Completed || State == EQuestState.Rewarded)
                return;

            // Task 돌면서 갱신 시도.
            foreach(var task in QuestTasks)
            {
                task.TryCompleteTask();
            }

            foreach(var task in QuestTasks)
            {
                if (task.IsCompleted == false)
                    return;
            }

            // 퀘스트 완료.
            State = EQuestState.Completed;

            // 퀘스트 완료 이벤트 뿌리기.
            Owner.BroadcastEvent(EBroadcastEventType.CompleteQuest, TemplateId, 1);
        }

        public void GiveReward()
        {
            if (State == EQuestState.Rewarded)
                return;

            State = EQuestState.Rewarded;

            // TODO 몬스터 RewardTable/ 퀘스트 보상 RewardTable 나누기 + 아이템 묶음 구현
            foreach (var rewardData in QuestData.RewardTableData.Rewards)
            {
                DBManager.RewardHero(Owner, rewardData);
            }

            Owner.RewardExpAndGold(QuestData.RewardTableData);
        }

        private void SetCurrentProcessingTask()
        {
            foreach (QuestTask task in QuestTasks)
            {
                // 진행중인 마지막 태스크 발견.
                if (task.IsCompleted == false)
                {
                    CurrentTask = task;
                    return;
                }
            }

            // 모든 태스크 종료.
            CurrentTask = null;
        }

        public bool CheckAndResetTaskDirtyFlag()
        {
            bool result = false;

            foreach (QuestTask task in QuestTasks)
            {
                if (task.DirtyFlag)
                    result = true;

                task.DirtyFlag = false;
            }

            return result;
        }

        #region Helpers
        public static Quest MakeQuest(QuestDb questDb, Hero owner)
        {
            if (DataManager.QuestDict.TryGetValue(questDb.TemplateId, out QuestData questData) == false)
                return null;

            Quest quest = new Quest(questDb, owner);
            return quest;
        }

        public int TemplateId
        {
            get { return Info.TemplateId; }
            set { Info.TemplateId = value; }
        }

        public EQuestState State
        {
            get { return Info.QuestState; }
            set { Info.QuestState = value; }
        }
        #endregion
    }
}
