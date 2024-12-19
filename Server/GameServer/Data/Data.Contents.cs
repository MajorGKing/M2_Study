using GameServer;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Data
{
    public class CreatureData
    {
        public int TemplateId;
        public string NameTextId;
        public string IconImage;
        public string PrefabName;
        public StatInfoWrapper Stat;

        public virtual bool Validate()
        {
            return true;
        }

    }

    [Serializable]
    public class StatInfoWrapper
    {
        public float hp;
        public float maxHp;
        public float speed;
        public float attack;
        public float defence;
        public int criRate;
        public int criDamage;

        public StatInfo StatInfo
        {
            get
            {
                return new StatInfo
                {
                    Hp = hp,
                    MaxHp = maxHp,
                    Speed = speed,
                    Attack = attack,
                    Defence = defence,
                    CriRate = criRate,
                    CriDamage = criDamage
                };
            }
        }
    }

    #region Hero
    public class HeroData : CreatureData
    {
        public string IconImageName;
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
