using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

namespace Scripts.Data
{
    #region Quest
    [CreateAssetMenu(fileName = "Assets/@Resources/Data/ScriptableObjectData/Quest/FILENAME", menuName = "Scriptable Objects/Quest", order = 0)]
    public class QuestData : ScriptableObject
    {
        public int TemplateId;
        public string Name;
        public int QuestPeriodType;
        public int RewardType;
        public int RewardDataId;
        public int RewardCount;    
        public string RewardIcon;
        public List<QuestTaskData> QuestTasks;
        public virtual bool Validate()
        {
            return true;
        }
    
    }
    
    [Serializable]
    public class QuestTaskData
    {
        public int TemplateId;
        public string DescriptionTextId;
        public int ObjectiveType;
        public string ObjectiveIcon;
        public int ObjectiveDataId;
        public int ObjectiveCount;
        public string DialogueId;
    }
    
    [Serializable]
    public class QuestDataLoader : ScriptableObject, ILoader<int, QuestData>
    {
        public List<QuestData> quests = new List<QuestData>();
    
        public Dictionary<int, QuestData> MakeDict()
        {
            Dictionary<int, QuestData> dict = new Dictionary<int, QuestData>();
            foreach (QuestData questData in quests)
                dict.Add(questData.TemplateId, questData);
    
            return dict;
        }
        
        public void SetDataList(List<QuestData> dataList)
        {
            quests = dataList;
        }
    
        public bool Validate()
        {
            bool validate = true;
    
            foreach (var hero in quests)
            {
                if (hero.Validate() == false)
                    validate = false;
            }
    
            return validate;
        }
    }
    
    #endregion
}