using System.Collections.Generic;
using System.Threading.Tasks;
using Data;
using Google.Protobuf.Collections;
using Google.Protobuf.Protocol;

namespace GameServer.Game
{
    public class QuestTask
    {
		public QuestTaskInfo TaskInfo { get; private set; }
		public QuestTaskData TaskData { get; private set; }
		public bool IsCompleted { get; private set; }

		public QuestTask(int templateId, QuestTaskInfo questTaskInfo)
        {
            if (Managers.Data.QuestTaskDict.TryGetValue(templateId, out QuestTaskData taskData) == false)
                return;

            TaskData = taskData;
            TaskInfo = questTaskInfo;

			TryUpdateTaskState();
		}

        public void UpdateQuestTaskInfo(QuestTaskInfo questTaskInfo)
        {
			TaskInfo = questTaskInfo;
		}

		public void TryUpdateTaskState()
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
		#endregion
	}
}
