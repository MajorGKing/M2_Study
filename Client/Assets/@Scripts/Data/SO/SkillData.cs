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

        // �ִϸ��̼� ��� ��ٸ��� ���� ������.
        public float DelayTime; // EventTime

        // ����ü�� ���� ���.
        public ProjectileData Projectile;

        // �������� ����?
        public EUseSkillTargetType UseSkillTargetType;

        // ȿ�� ��� ������? (0�̸� ���� ��ų)
        public int GatherTargetRange;
        public string GatherTargetPrefabName; // AoE

        // �Ǿƽĺ�
        public ETargetFriendType TargetFriendType;

        // � ȿ����?
        public EffectData EffectData;

        // ���� ���� ��ų.
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