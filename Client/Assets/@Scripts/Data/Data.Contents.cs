using System;
using System.Collections.Generic;
using UnityEngine;
using static Define;

namespace Data
{
    #region TextData
    [Serializable]
    public class TextData
    {
        public string TemplateId;
        public string KOR;
    }

    [Serializable]
    public class TextDataLoader : ILoader<string, TextData>
    {
        public List<TextData> texts = new List<TextData>();

        public Dictionary<string, TextData> MakeDict()
        {
            Dictionary<string, TextData> dict = new Dictionary<string, TextData>();
            foreach (TextData text in texts)
                dict.Add(text.TemplateId, text);

            return dict;
        }

        public bool Validate()
        {
            return true;
        }
    }
    #endregion

    [Serializable]
    public class CreatureData
    {
        public int TemplateId;
        public string NameTextId;
        public string IconImage;
        public string SkeletonDataId;

        public virtual bool Validate()
        {
            return true;
        }
    }
    #region HeroData
    [Serializable]
    public class HeroData : CreatureData
    {
        public string DescriptionTextId;

        public override bool Validate()
        {
            bool validate = base.Validate();
            return validate;
        }
    }

    [Serializable]
    public class HeroDataLoader : ILoader<int, HeroData>
    {
        public List<HeroData> Heroes = new List<HeroData>();

        public Dictionary<int, HeroData> MakeDict()
        {
            Dictionary<int, HeroData> dict = new Dictionary<int, HeroData>();
            foreach (HeroData heroData in Heroes)
                dict.Add(heroData.TemplateId, heroData);

            return dict;
        }

        public bool Validate()
        {
            bool validate = true;

            foreach (var hero in Heroes)
            {
                if (hero.Validate() == false)
                    validate = false;
            }

            return validate;
        }
    }
    #endregion
}