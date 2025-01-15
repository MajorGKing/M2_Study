using GameServer;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Data
{
    public class BaseData
    {
        public int TemplateId;
        public string Name;//개발용
        public string NameTextId;
        public string DescriptionTextID;
        public string IconImage;
        public string PrefabName;
    }
    public class CreatureData : BaseData
    {
        public StatInfoWrapper Stat;
    }

    [Serializable]
    public class StatInfoWrapper
    {
        public float MaxHp;
        public float Hp;
        public float HpRegen;
        public float MaxMp;
        public float Mp;
        public float MpRegen;
        public float Attack;
        public float Defence;
        public float MissChance;
        public float AttackSpeed;
        public float MoveSpeed;
        public float CriRate;
        public float CriDamage;
        public int Str;
        public int Dex;
        public int Int;
        public int Con;
        public int Wis;

        public StatInfo StatInfo
        {
            get
            {
                return new StatInfo
                {
                    MaxHp = this.MaxHp,
                    Hp = this.Hp,
                    HpRegen = this.HpRegen,
                    MaxMp = this.MaxMp,
                    Mp = this.Mp,
                    MpRegen = this.MpRegen,
                    Attack = this.Attack,
                    Defence = this.Defence,
                    Dodge = this.MissChance,
                    AttackSpeed = this.AttackSpeed,
                    MoveSpeed = this.MoveSpeed,
                    CriRate = this.CriRate,
                    CriDamage = this.CriDamage,
                    Str = this.Str,
                    Dex = this.Dex,
                    Int = this.Int,
                    Con = this.Con,
                    Wis = this.Wis,
                };
            }
        }
    }

    #region Hero
    public class HeroData : CreatureData
    {
        public string IconImageName;

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

    #region Monster
    public class MonsterData : CreatureData
    {
        public bool IsBoss = false;
        public bool IsAggressive;
        public int ExtraCells;

        public string IconImageName;

        public SkillData MainSkill;
        public SkillData SkillA;
        public SkillData SkillB;

        // AI
        public int SearchCellDist;
        public int ChaseCellDist;
        public int PatrolCellDist;

        // 스폰 정보

        // 드롭아이템

        // ItemHolder items(경험치는 홀더안에?)
    }

    [Serializable]
    public class MonsterDataLoader : ILoader<int, MonsterData>
    {
        public List<MonsterData> monsters = new List<MonsterData>();
        public Dictionary<int, MonsterData> MakeDict()
        {
            Dictionary<int, MonsterData> dict = new Dictionary<int, MonsterData>();
            foreach (MonsterData monster in monsters)
                dict.Add(monster.TemplateId, monster);

            return dict;
        }

        public bool Validate()
        {
            bool validate = true;
            return validate;
        }
    }
    #endregion

    #region Projectile

    public class ProjectileData : BaseData
    {
        public float Duration;
        public float ProjRange;
        public float ProjSpeed; 
    }

    [Serializable]
    public class ProjectileDataLoader : ILoader<int, ProjectileData>
    {
        public List<ProjectileData> datas = new List<ProjectileData>();

        public Dictionary<int, ProjectileData> MakeDict()
        {
            Dictionary<int, ProjectileData> dict = new Dictionary<int, ProjectileData>();
            foreach (ProjectileData data in datas)
                dict.Add(data.TemplateId, data);

            return dict;
        }

        public bool Validate()
        {
            bool validate = true;
            return validate;
        }
    }
    #endregion

    #region Skill

    public class SkillData : BaseData
    {
        public ESkillType SkillType;
        public string DescriptionTextId;
        public string IconLabel;
        public float Cooldown;
        public int SkillRange;
        public string AnimName;
        public int Cost;

        // 애니메이션 모션 기다리기 위한 딜레이.
        public float DelayTime; // EventTime

        // 투사체를 날릴 경우.
        public ProjectileData Projectile;

        // 누구한테 시전?
        public EUseSkillTargetType UseSkillTargetType;

        // 효과 대상 범위는? (0이면 단일 스킬)
        public int GatherTargetRange;
        public string GatherPrefabName; // AoE

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

    #region Effect
    public class EffectData :BaseData
    {
        public string SoundLabel;

        // 효과 분류.
        public EEffectType EffectType;

        // 즉발 vs 주기적 vs 영구적.
        public EDurationPolicy DurationPolicy;
        public float Duration;

        // EFFECT_TYPE_DAMAGE
        public float DamageValue;

        // EFFECT_TYPE_HEAL
        public float HealValue;

        // EFFECT_TYPE_BUFF_STAT
        public EStatType StatType;
        public float StatAddValue;

        // EFFECT_TYPE_BUFF_LIFE_STEAL
        public float LifeStealValue;

        // EFFECT_TYPE_BUFF_LIFE_STUN
        public float StunValue;
    }

    [Serializable]
    public class EffectDataLoader : ILoader<int, EffectData>
    {
        public List<EffectData> datas = new List<EffectData>();

        public Dictionary<int, EffectData> MakeDict()
        {
            Dictionary<int, EffectData> dict = new Dictionary<int, EffectData>();
            foreach (EffectData data in datas)
                dict.Add(data.TemplateId, data);

            return dict;
        }

        public bool Validate()
        {
            bool validate = true;
            return validate;
        }
    }

    #endregion

    #region AOE
    public class AOEData
    {
        public int TemplateId;
        public string Name;
        public string PrefabName;
        public string SoundLabel;
        public List<EffectData> AllyEffects;
        public List<EffectData> EnemyEffects;
        public int Range;
    }

    [Serializable]
    public class AOEDataLoader : ILoader<int, AOEData>
    {
        public List<AOEData> datas = new List<AOEData>();

        public Dictionary<int, AOEData> MakeDict()
        {
            Dictionary<int, AOEData> dict = new Dictionary<int, AOEData>();
            foreach (AOEData data in datas)
                dict.Add(data.TemplateId, data);

            return dict;
        }

        public bool Validate()
        {
            bool validate = true;
            return validate;
        }
    }
    #endregion

    #region SpawningPool
    [Serializable]
    public class RespawnInfo
    {
        public int TemplateId; // MonsterData의 TemplateId
        public MonsterData MonsterData;
        public int Count;
        public ERespawnType RespawnType;
        public float Interval;
        public int respawnTime;
    }

    [Serializable]
    public class SpawningPoolData
    {
        public int id;
        public int size;
        public List<RespawnInfo> monsters;
    }

    [Serializable]
    public class SpawningPoolDataLoader : ILoader<int, SpawningPoolData>
    {
        public List<SpawningPoolData> spawningPools = new List<SpawningPoolData> { };

        public Dictionary<int, SpawningPoolData> MakeDict()
        {
            Dictionary<int, SpawningPoolData> dict = new Dictionary<int, SpawningPoolData>();
            foreach (SpawningPoolData spawningPool in spawningPools)
            {
                dict.Add(spawningPool.id, spawningPool);
            }
            return dict;
        }
    }
    #endregion

    #region BaseStat
    public class BaseStatData
    {
        public int Level;
        public int Attack;
        public int MaxHp;
        public int MaxMp;
        public int HpRegen;
        public int MpRegen;
        public int Def;
        public int Dodge;
        public int AtkSpeed;
        public int MoveSpeed;
        public float CriRate;
        public float CriDamage;
        public int Str;
        public int Dex;
        public int Int;
        public int Con;
        public int Wis;
        public int Exp;
    }

    [Serializable]
    public class BaseStatDataLoader : ILoader<int, BaseStatData>
    {
        public List<BaseStatData> baseStatDatas = new List<BaseStatData>();

        public Dictionary<int, BaseStatData> MakeDict()
        {
            Dictionary<int, BaseStatData> dict = new Dictionary<int, BaseStatData>();
            foreach (BaseStatData stat in baseStatDatas)
                dict.Add(stat.Level, stat);

            return dict;
        }

        public bool Validate()
        {
            return true;
        }
    }
    #endregion

}
