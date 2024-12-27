using System;
using System.Collections.Generic;
using Data;
using Google.Protobuf.Protocol;
using UnityEngine;

namespace Scripts.Data.SO
{
    #region Hero
    [CreateAssetMenu(fileName = "Assets/@Resources/Data/ScriptableObjectData/Hero/FILENAME", menuName = "Scriptable Objects/Hero", order = 0)]
    public class HeroData : CreatureData
    {
        public EHeroClass HeroClass;
        public SkillData MainSkill;
        public SkillData SkillA;
        public SkillData SkillB;
        public SkillData SkillC;
        public SkillData SkillD;
        public SkillData SkillE;
    }

    [Serializable]
    public class HeroDataLoader : ILoader<int, HeroData>
    {
        public List<HeroData> heroes = new List<HeroData>();
    
        public Dictionary<int, HeroData> MakeDict()
        {
            Dictionary<int, HeroData> dict = new Dictionary<int, HeroData>();
            foreach (HeroData hero in heroes)
                dict.Add(hero.TemplateId, hero);
    
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