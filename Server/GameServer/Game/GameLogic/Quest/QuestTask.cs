using Google.Protobuf.Collections;
using Google.Protobuf.Protocol;
using Server.Data;

namespace GameServer.Game
{
    public class QuestTask : IBroadcastEventListener
    {
        public QuestTaskInfo TaskInfo { get; private set; }
        public QuestTaskData TaskData { get; private set; }
        public Quest Owner { get; private set; }

        public bool IsCompleted { get; private set; }

        // 진행 사항에 바뀐 내역이 있는지 확인하는 용도.
        public bool DirtyFlag { get; set; } = false;

        public QuestTask(int templateId, QuestTaskDb taskDb, Quest owner)
        {
            if (DataManager.QuestTaskDict.TryGetValue(templateId, out QuestTaskData taskData) == false)
                return;

            TaskInfo = new QuestTaskInfo();

            // Objective 추가.
            for (int i = 0; i < taskDb.ObjectiveCounts.Count; i++)
                TaskInfo.Objectives.Add(taskDb.ObjectiveTemplateIds[i], taskDb.ObjectiveCounts[i]);

            TaskData = taskData;
            Owner = owner;

            // TaskState는 저장하지 않고 기존의 데이터로 다시 복원한다.
            TryCompleteTask();
        }

        public void OnBroadcastEvent(EBroadcastEventType type, int targetId, int count)
        {
            // 현재 진행중인 태스크만 클리어 가능. (기획 사항)
            if (Owner.CurrentTask != this)
                return;

            switch(type)
            {
                case EBroadcastEventType.KillTarget:
                    TryUpdateQuestTaskProgress(EQuestTaskType.KillTarget, targetId, count);
                    break;
                case EBroadcastEventType.InteractWithNpc:
                    TryUpdateQuestTaskProgress(EQuestTaskType.InteractWithNpc, targetId, 1);
                    break;
                case EBroadcastEventType.CollectItem:
                    TryUpdateQuestTaskProgress(EQuestTaskType.CollectItem, targetId, count);
                    break;
            }
        }

        private void TryUpdateQuestTaskProgress(EQuestTaskType taskType, int templateId = 0, int addCount = 0)
        {
            // 1. 나랑 상관없는 이벤트라면 무시.
            if (taskType != TaskData.TaskType)
                return;

            // 2. 조건과 무관하면 스킵.
            if (Objectives.ContainsKey(templateId) == false)
                return;

            // 3. 목표 달성 횟수 추가.
            Objectives[templateId] += addCount;
            DirtyFlag = true;

            // 4. 완료 상태 확인.
            TryCompleteTask();
        }

        public void TryCompleteTask()
        {
            // 완료했으면 끝.
            if (IsCompleted)
                return;

            // 하나라도 목표보다 작게 달성했으면 미완성.
            foreach (var pair in TaskData.Objectives)
            {
                if (Objectives.ContainsKey(pair.Key) == false)
                    return;

                int currentCount = Objectives[pair.Key];
                int objectiveCount = pair.Value;
                if (currentCount < objectiveCount)
                    return;
            }

            // 태스크 완성.
            IsCompleted = true;
        }

        #region Helpers
        public MapField<int, int> Objectives { get { return TaskInfo.Objectives; } }
        public int TemplateId { get { return TaskData.TemplateId; } }

        public bool IsNeededItemToProceedQuestTask(int templateId)
        {
            // 수집 태스크인지.
            if (TaskData.TaskType != EQuestTaskType.CollectItem)
                return false;

            // 해당 아이템이 필요한 아이템인지.
            if (Objectives.ContainsKey(templateId) == false)
                return false;

            // 수량이 부족한지.
            int currentCount = Objectives[templateId];
            int objectiveCount = templateId;
            if (currentCount >= objectiveCount)
                return false;

            return true;
        }
        #endregion
    }
}
