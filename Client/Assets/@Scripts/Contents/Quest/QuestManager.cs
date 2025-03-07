using Google.Protobuf.Protocol;
using System.Collections.Generic;
using System.Linq;
using Scripts.Data.SO;
using UnityEngine;

namespace GameServer.Game
{
    public class QuestManager
    {
        public Dictionary<int, Quest> AllQuests = new Dictionary<int, Quest>();

        public void Init()
        {
        }

        public void HandleEnterGame(S_EnterGame enterGame)
        {
            Clear();

            foreach (QuestInfo questInfo in enterGame.Quests)
                AddQuest(questInfo);
        }

        public void HandeAddOrUpdateQuest(S_AddOrUpdateQuest questPacket)
        {
            QuestInfo questInfo = questPacket.QuestInfo;

			if (AllQuests.TryGetValue(questInfo.TemplateId, out Quest quest) == false)
				AddQuest(questInfo);
            else
				quest.UpdateQuest(questInfo);

            Managers.UI.GetSceneUI<UI_GameScene>().RefreshUI();
        }

        public Quest AddQuest(QuestInfo questInfo)
        {
            Quest quest = Quest.MakeQuest(questInfo);
            if (quest == null)
                return null;

            AllQuests.Add(quest.TemplateId, quest);

            return quest;
        }

        public void Clear()
        {
            AllQuests.Clear();
        }

        #region Helper

        public Quest GetQuest(int templateId)
        {
            if (AllQuests.TryGetValue(templateId, out Quest quest))
            {
                return quest;
            }

            return null;
        }

        public QuestTask GetCurrentQuestTask()
        {
            foreach (Quest quest in GetProcessingQuests())
            {
                if (quest.QuestData.Type != EQuestType.Main)
                    continue;

                if (quest.CurrentTask == null)
                    continue;
                
                return quest.CurrentTask;
            }

            return null;
        }

        public DialogueData GetDialogue(EQuestTaskType taskType = EQuestTaskType.InteractWithNpc, int npcTemplateId = 0)
        {
            QuestTask task = GetCurrentQuestTask();
            if (task == null)
                return null;
            
            if (task.TaskData.TaskType != taskType)
                return null;

            if (task.TaskData.ObjectiveDataIds.FirstOrDefault() != npcTemplateId)
                return null;

            return task.TaskData.DialogueData;
        }

        public List<Quest> GetProcessingQuests()
        {
            return AllQuests?.Values.Where(q => q.State == EQuestState.Processing).ToList();
        }
        
        #endregion
    }
}
