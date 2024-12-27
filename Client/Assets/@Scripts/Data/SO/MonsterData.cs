using System;
using System.Collections.Generic;
using Data;
using Google.Protobuf.Protocol;
using UnityEngine;

namespace Scripts.Data.SO
{
    #region Monster
    [CreateAssetMenu(fileName = "Assets/@Resources/Data/ScriptableObjectData/Monster/FILENAME", menuName = "Scriptable Objects/Monster", order = 0)]
    public class MonsterData : CreatureData
    {
        public int Level;
        public bool IsBoss = false;
        public bool IsAggressive;
        public int ExtraCells;

        public SkillData MainSkill;
        public SkillData SkillA;
        public SkillData SkillB;

        //AI
        public int SearchCellDist;
        public int ChaseCellDist;
        public int PatrolCellDist;
        
        //스폰 정보

        //드롭아이템

    }

    [Serializable]
    public class MonsterDataLoader : ILoader<int, MonsterData>
    {
        public List<MonsterData> monsters = new List<MonsterData>();
    
        public Dictionary<int, MonsterData> MakeDict()
        {
            Dictionary<int, MonsterData> dict = new Dictionary<int, MonsterData>();
            foreach (MonsterData monster in monsters)
            {
                dict.Add(monster.TemplateId, monster);
            }

    
            return dict;
        }
        
        public bool Validate()
        {
            bool validate = true;
            return validate;
        }
    }
    #endregion

}