using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

namespace Scripts.Data
{
    #region Skill
   
    [CreateAssetMenu(fileName = "Assets/@Resources/Data/ScriptableObjectData/Skill/FILENAME", menuName = "Scriptable Objects/Skill", order = 0)]
    public class SkillData : ScriptableObject
    {
        public int TemplateId;
        public string Name;
        public string NameTextId;
        public string ClassName;
        public string Description;
        public string DescriptionTextId;
        public ProjectileData Projectile;
        public string IconLabel;
        public string AnimName;
        public float CoolTime;
        public float Duration;
        public string CastingAnimname;
        public string CastingSound;
        public float SkillRange;
        public int TargetCount;
        public List<EffectData> Effects;
        public SkillData NextLevelSkill;
        public List<AOEData> AoEs;
    }

    [Serializable]
    public class SkillDataLoader : ILoader<int, SkillData>
    {
        public List<SkillData> skills = new List<SkillData>();
    
        public Dictionary<int, SkillData> MakeDict()
        {
            Dictionary<int, SkillData> dict = new Dictionary<int, SkillData>();
            foreach (SkillData skillData in skills)
                dict.Add(skillData.TemplateId, skillData);
    
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