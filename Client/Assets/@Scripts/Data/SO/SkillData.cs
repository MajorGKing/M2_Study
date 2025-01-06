using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using Scripts.Data.SO;
using UnityEngine;

namespace Scripts.Data
{
    #region Skill
   
    [CreateAssetMenu(fileName = "Assets/@Resources/Data/ScriptableObjectData/Skill/FILENAME", menuName = "Scriptable Objects/Skill", order = 0)]
    public class SkillData : BaseData
    {
        public ESkillType SkillType;
        public EItemGrade SkillGrade;
        public float Cooldown;
        public int SkillRange;
        public string AnimName;

        // 애니메이션 모션 기다리기 위한 딜레이.
        public float DelayTime; // EventTime

        // 투사체를 날릴 경우.
        public ProjectileData Projectile;

        // 누구한테 시전?
        public EUseSkillTargetType UseSkillTargetType;

        // 효과 대상 범위는? (0이면 단일 스킬)
        public int GatherTargetRange;
        public string GatherTargetPrefabName; // AoE

        // 피아식별
        public ETargetFriendType TargetFriendType;

        // 어떤 효과를?
        public EffectData EffectData;

        // 다음 레벨 스킬.
        public SkillData NextLevelSkill;
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