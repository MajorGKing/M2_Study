using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

namespace Data
{
    //���� �Ľ̿� ������ ��Ʈ����Ʈ ����
    [AttributeUsage(AttributeTargets.Field)]
    public class ExcludeFieldAttribute : Attribute
    {
    }

    #region BaseData

    public class BaseData
    {
        public int TemplateId;
        public string Name; //���߿�
        public string NameTextId;
        public string DescriptionTextID;
        public string IconImage;
        public string PrefabName;
    }

    public class CreatureData : BaseData
    {
        public int MaxHp;
        public int HpRegen;
        public int MaxMp;
        public int MpRegen;
        public int Attack;
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

        [ExcludeField]
        public StatInfo Stat;
        public virtual bool Validate()
        {
            return true;
        }
    }

    public class ItemData : BaseData
    {
        public EItemType Type;
        public EItemSubType SubType;
        public EItemGrade Grade;
        public int MaxStack;

        [ExcludeField]
        public bool Stackable;
    }

    public class NpcData : ScriptableObject
    {
        public int TemplateId;
        public string Name; //���߿�
        public string NameTextId;
        public string DescriptionTextID;
        public string IconImage;
        public string PrefabName;
        public ENpcType NpcType;
        public int ExtraSize;
        public int Range;

        public int OwnerRoomId;
        public int SpawnPosX;
        public int SpawnPosY;
    }

    #endregion

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

    #region Hero

    public class HeroData : CreatureData
    {
        public EHeroClass HeroClass;
        public List<int> SkillDataIds;

        [ExcludeField]
        public List<SkillData> SkillDatas = new List<SkillData>();
        [ExcludeField]
        public Dictionary<ESkillSlot, SkillData> SkillMap = new Dictionary<ESkillSlot, SkillData>();
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

            foreach (var hero in heroes)
            {
                hero.Stat = new StatInfo()
                {
                    MaxHp = hero.MaxHp,
                    HpRegen = hero.HpRegen,
                    MaxMp = hero.MaxMp,
                    MpRegen = hero.MpRegen,
                    Attack = hero.Attack,
                    Defence = hero.Def,
                    Dodge = hero.Dodge,
                    AttackSpeed = hero.AtkSpeed,
                    MoveSpeed = hero.MoveSpeed,
                    CriRate = hero.CriRate,
                    CriDamage = hero.CriDamage,
                    Str = hero.Str,
                    Dex = hero.Dex,
                    Int = hero.Int,
                    Con = hero.Con,
                    Wis = hero.Wis
                };

                //Skill
                for (int i = 0; i < hero.SkillDataIds.Count; i++)
                {
                    if (Managers.Data.SkillDict.TryGetValue(hero.SkillDataIds[i], out SkillData skillData))
                    {
                        hero.SkillDatas.Add(skillData);

                        // �ڵ����� SkillMap�� �߰�
                        if (i + 1 < Enum.GetValues(typeof(ESkillSlot)).Length)
                        {
                            hero.SkillMap[(ESkillSlot)i + 1] = skillData;
                        }
                    }
                }
            }

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
        public List<int> SkillDataIds;

        //AI
        public int SearchCellDist;
        public int ChaseCellDist;
        public int PatrolCellDist;

        //Drop
        public int DropTableId;

        [ExcludeField]
        public List<SkillData> SkillDatas = new List<SkillData>();
        [ExcludeField]
        public Dictionary<ESkillSlot, SkillData> SkillMap = new Dictionary<ESkillSlot, SkillData>();
        [ExcludeField]
        public DropTableData DropTable;
        //���� ����
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
            foreach (var monster in monsters)
            {
                monster.Stat = new StatInfo()
                {
                    MaxHp = monster.MaxHp,
                    HpRegen = monster.HpRegen,
                    MaxMp = monster.MaxMp,
                    MpRegen = monster.MpRegen,
                    Attack = monster.Attack,
                    Defence = monster.Def,
                    Dodge = monster.Dodge,
                    AttackSpeed = monster.AtkSpeed,
                    MoveSpeed = monster.MoveSpeed,
                    CriRate = monster.CriRate,
                    CriDamage = monster.CriDamage,
                    Str = monster.Str,
                    Dex = monster.Dex,
                    Int = monster.Int,
                    Con = monster.Con,
                    Wis = monster.Wis
                };

                //Skill
                for (int i = 0; i < monster.SkillDataIds.Count; i++)
                {
                    if (Managers.Data.SkillDict.TryGetValue(monster.SkillDataIds[i], out SkillData skillData))
                    {
                        monster.SkillDatas.Add(skillData);

                        // �ڵ����� SkillMap�� �߰�
                        if (i + 1 < Enum.GetValues(typeof(ESkillSlot)).Length)
                        {
                            monster.SkillMap[(ESkillSlot)i + 1] = skillData;
                        }
                    }
                }

                Managers.Data.DropTableDict.TryGetValue(monster.DropTableId, out monster.DropTable);
            }
            return validate;
        }
    }

    #endregion

    #region Equipment

    public class EquipmentData : ItemData
    {
        public EItemSlotType SlotType;
        public bool canTrade;
        public bool canDelete;
        public bool canStorable;
        public int EffectDataId;
        public int SafeEnhancementLevel;
        public int NextLevelItemDataId;

        [ExcludeField]
        public EffectData EffectData;
        [ExcludeField]
        public ItemData NextLevelItem;
    }

    [Serializable]
    public class EquipmentDataLoader : ILoader<int, EquipmentData>
    {
        public List<EquipmentData> items = new List<EquipmentData>();

        public Dictionary<int, EquipmentData> MakeDict()
        {
            Dictionary<int, EquipmentData> dict = new Dictionary<int, EquipmentData>();
            foreach (EquipmentData item in items)
                dict.Add(item.TemplateId, item);

            return dict;
        }

        public bool Validate()
        {
            bool validate = true;

            foreach (var equipmentData in items)
            {
                Managers.Data.EffectDict.TryGetValue(equipmentData.EffectDataId, out equipmentData.EffectData);
                Managers.Data.ItemDict.TryGetValue(equipmentData.NextLevelItemDataId, out equipmentData.NextLevelItem);

            }
            return validate;
        }
    }

    #endregion

    #region Consumable

    public class ConsumableData : ItemData
    {
        public int EffectId;
        public int CoolTime;
        public EConsumableGroupType ConsumableGroupType;

        [ExcludeField]
        public EffectData EffectData;
    }

    [Serializable]
    public class ConsumableDataLoader : ILoader<int, ConsumableData>
    {
        public List<ConsumableData> items = new List<ConsumableData>();

        public Dictionary<int, ConsumableData> MakeDict()
        {
            Dictionary<int, ConsumableData> dict = new Dictionary<int, ConsumableData>();
            foreach (ConsumableData item in items)
                dict.Add(item.TemplateId, item);

            return dict;
        }

        public bool Validate()
        {
            bool validate = true;

            foreach (ConsumableData item in items)
            {
                item.Stackable = true;
                Managers.Data.EffectDict.TryGetValue(item.EffectId, out item.EffectData);
            }

            return validate;
        }
    }

    #endregion

    #region DropTableData

    [Serializable]
    public class DropTableData
    {
        public int TemplateId;
        public string Name;
        public int RewardGold;
        public int RewardExp;
        public List<int> RewardDataIds;

        [ExcludeField]
        public List<RewardData> Rewards;
    }

    [Serializable]
    public class DropTableDataLoader : ILoader<int, DropTableData>
    {
        public List<DropTableData> dropTables = new List<DropTableData>();

        public Dictionary<int, DropTableData> MakeDict()
        {
            Dictionary<int, DropTableData> dict = new Dictionary<int, DropTableData>();
            foreach (DropTableData dropTableData in dropTables)
            {
                dict.Add(dropTableData.TemplateId, dropTableData);
            }

            return dict;
        }

        public bool Validate()
        {
            foreach (var dropTable in dropTables)
            {
                dropTable.Rewards = new List<RewardData>();
                foreach (var id in dropTable.RewardDataIds)
                {
                    RewardData reward;
                    if (Managers.Data.RewardDict.TryGetValue(id, out reward))
                    {
                        dropTable.Rewards.Add(reward);
                    }
                }
            }
            return true;
        }
    }

    #endregion

    #region Reward

    public class RewardData
    {
        public int TemplateId;
        public string Name;
        public int ItemTemplateId;
        public int Probability; // 100����
        public int Count;

        [ExcludeField]
        public ItemData Item;
    }

    [Serializable]
    public class RewardDataLoader : ILoader<int, RewardData>
    {
        public List<RewardData> rewards = new List<RewardData>();

        public Dictionary<int, RewardData> MakeDict()
        {
            Dictionary<int, RewardData> dict = new Dictionary<int, RewardData>();
            foreach (RewardData rewardData in rewards)
            {
                dict.Add(rewardData.TemplateId, rewardData);
            }

            return dict;
        }

        public bool Validate()
        {
            foreach (var reward in rewards)
            {
                if (Managers.Data.ItemDict.TryGetValue(reward.ItemTemplateId, out reward.Item))
                {
                }
                else
                {
                    Debug.LogError($"������ �����͸� ã�� �� �����ϴ�.");
                    return false;
                }
            }

            return true;
        }
    }

    #endregion

    #region EffectData

    public struct StatValuePair
    {
        public EStatType StatType;
        public float AddValue;
    }

    public class EffectData : BaseData
    {
        public string SoundLabel;

        public EEffectType EffectType;
        // ��� vs �ֱ��� vs ������.
        public EDurationPolicy DurationPolicy;
        public float Duration;
        // EFFECT_TYPE_DAMAGE
        public float DamageValue;
        // EFFECT_TYPE_BUFF_STAT
        public List<EStatType> StatType;
        public List<float> AddValue;
        // EFFECT_TYPE_BUFF_LIFE_STEAL
        public float LifeStealValue;
        // EFFECT_TYPE_BUFF_LIFE_STUN
        public float StunValue;

        [ExcludeField]
        public List<StatValuePair> StatValues = new List<StatValuePair>();
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

            foreach (var data in datas)
            {
                data.StatValues = new List<StatValuePair>();
                for (int i = 0; i < data.StatType.Count; i++)
                {
                    data.StatValues.Add(new StatValuePair()
                    {
                        StatType = data.StatType[i],
                        AddValue = data.AddValue[i],
                    });
                }
            }
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
        public EItemGrade SkillGrade;
        public float Cooltime;
        public int SkillRange;
        public string AnimName;
        public int Cost;
        // �ִϸ��̼� ��� ��ٸ��� ���� ������.
        public float DelayTime; // EventTime

        // ����ü�� ���� ���.
        public int ProjectileId;

        // �������� ����?
        public EUseSkillTargetType UseSkillTargetType;

        // ȿ�� ��� ������? (0�̸� ���� ��ų)
        public int GatherTargetRange;
        public string GatherTargetPrefabName; // AoE

        // �Ǿƽĺ�
        public ETargetFriendType TargetFriendType;

        // � ȿ����?
        public int EffectDataId;

        // ���� ���� ��ų.
        public int NextLevelSkillId;

        [ExcludeField]
        public SkillData NextLevelSkill;
        [ExcludeField]
        public ProjectileData ProjectileData;
        [ExcludeField]
        public EffectData EffectData;
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

            foreach (var skill in skills)
            {
                Managers.Data.SkillDict.TryGetValue(skill.NextLevelSkillId, out skill.NextLevelSkill);
                Managers.Data.ProjectileDict.TryGetValue(skill.ProjectileId, out skill.ProjectileData);
                Managers.Data.EffectDict.TryGetValue(skill.EffectDataId, out skill.EffectData);
            }
            return validate;
        }
    }

    #endregion

    #region Quest
    [CreateAssetMenu(fileName = "Assets/@Resources/Data/ScriptableObjectData/Quest/FILENAME", menuName = "Scriptable Objects/Quest", order = 0)]
    public class QuestData : BaseData
    {
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