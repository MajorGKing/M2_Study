using GameServer;
using Google.Protobuf.Protocol;

namespace Server.Data
{
    //엑셀 파싱에 제외할 어트리뷰트 정의
    [AttributeUsage(AttributeTargets.Field)]
    public class ExcludeFieldAttribute : Attribute
    {
    }

    #region BaseData

    public class BaseData
    {
        public int TemplateId;
        public string Name; //개발용
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

    public class NpcData
    {
        public int TemplateId;
        public string Name; //개발용
        public string NameTextId;
        public string DescriptionTextID;
        public string IconImage;
        public string PrefabName;
        public ENpcType NpcType;
        public int ExtraCells;
        public int Range;

        public int OwnerRoomId;
        public int SpawnPosX;
        public int SpawnPosY;

        public int DialogueId;

        public PositionInfo GetSpawnPosition()
        {
            PositionInfo positionInfo = new PositionInfo()
            {
                RoomId = OwnerRoomId,
                PosX = SpawnPosX,
                PosY = SpawnPosY
            };
            return positionInfo;
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
                    if (DataManager.SkillDict.TryGetValue(hero.SkillDataIds[i], out SkillData skillData))
                    {
                        hero.SkillDatas.Add(skillData);

                        // 자동으로 SkillMap에 추가
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
        public int RewardTableId;

        [ExcludeField]
        public List<SkillData> SkillDatas = new List<SkillData>();
        [ExcludeField]
        public Dictionary<ESkillSlot, SkillData> SkillMap = new Dictionary<ESkillSlot, SkillData>();
        [ExcludeField]
        public RewardTableData RewardTable;
        //스폰 정보
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
                    if (DataManager.SkillDict.TryGetValue(monster.SkillDataIds[i], out SkillData skillData))
                    {
                        monster.SkillDatas.Add(skillData);

                        // 자동으로 SkillMap에 추가
                        if (i + 1 < Enum.GetValues(typeof(ESkillSlot)).Length)
                        {
                            monster.SkillMap[(ESkillSlot)i + 1] = skillData;
                        }
                    }
                }

                DataManager.RewardTableDict.TryGetValue(monster.RewardTableId, out monster.RewardTable);
            }
            return validate;
        }
    }

    #endregion

    #region Equipment

    public class EquipmentData : ItemData
    {
        public EItemSlotType SlotType;
        public bool CanTrade;
        public bool CanDelete;
        public bool CanStorable;
        public int MaxHpBonus;
        public int AttackBonus;
        public int DefenceBonus;
        public int EffectDataId;
        public int SafeEnchantLevel;
        public int NextLevelItemDataId;

        [ExcludeField]
        public int BaseItemDataId;
        [ExcludeField]
        public EffectData EffectData;
        [ExcludeField]
        public EquipmentData NextLevelItem;
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
                DataManager.EffectDict.TryGetValue(equipmentData.EffectDataId, out equipmentData.EffectData);
                DataManager.EquipmentDict.TryGetValue(equipmentData.NextLevelItemDataId, out equipmentData.NextLevelItem);

                //equipmentData.BaseItemDataId = FindBaseId(equipmentData);
            }

			// Cache.
			foreach (var equipmentData in items)
            {
                if (equipmentData.NextLevelItemDataId != 0)
					_baseCache[equipmentData.NextLevelItemDataId] = equipmentData.TemplateId;
            }

            // 실제로 찾기.
			foreach (var equipmentData in items)
				SetBaseItem(equipmentData);

			return validate;
        }

        private Dictionary<int, int> _baseCache = new Dictionary<int, int>();

		private void SetBaseItem(EquipmentData data)
		{
            if (data == null)
                return;

            if (_baseCache.ContainsKey(data.TemplateId) == false)
            {
                // 기본 아이템이라면, BaseItemDataId가 자기 자신이다.
                _baseCache[data.TemplateId] = data.TemplateId;
				data.BaseItemDataId = data.TemplateId;               
            }
            else
            {
                data.BaseItemDataId = _baseCache[data.TemplateId];
            }

			// 다음 아이템의 기본 아이템을 설정.
			if (DataManager.ItemDict.TryGetValue(data.NextLevelItemDataId, out ItemData next))
			{
				_baseCache[data.NextLevelItemDataId] = _baseCache[data.TemplateId];
				SetBaseItem(next as EquipmentData);
			}
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
                DataManager.EffectDict.TryGetValue(item.EffectId, out item.EffectData);
            }

            return validate;
        }
    }

    #endregion

    #region Collectible
    public class CollectibleData : ItemData
    { }

    [Serializable]
    public class CollectibleDataLoader : ILoader<int, CollectibleData>
    {
        public List<CollectibleData> items = new List<CollectibleData>();

        public Dictionary<int, CollectibleData> MakeDict()
        {
            Dictionary<int, CollectibleData> dict = new Dictionary<int, CollectibleData>();
            foreach (CollectibleData item in items)
                dict.Add(item.TemplateId, item);

            return dict;
        }

        public bool Validate()
        {
            bool validate = true;

            foreach(CollectibleData item in items)
            {
                item.Stackable = true;
            }

            return validate;
        }
    }
    #endregion

    #region RewardTableData

    [Serializable]
    public class RewardTableData
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
    public class RewardTableDataLoader : ILoader<int, RewardTableData>
    {
        public List<RewardTableData> rewardTables = new List<RewardTableData>();

        public Dictionary<int, RewardTableData> MakeDict()
        {
            Dictionary<int, RewardTableData> dict = new Dictionary<int, RewardTableData>();
            foreach (RewardTableData rewardTableData in rewardTables)
            {
                dict.Add(rewardTableData.TemplateId, rewardTableData);
            }

            return dict;
        }

        public bool Validate()
        {
            foreach (var rewardTable in rewardTables)
            {
                rewardTable.Rewards = new List<RewardData>();
                foreach (var id in rewardTable.RewardDataIds)
                {
                    RewardData reward;
                    if (DataManager.RewardDict.TryGetValue(id, out reward))
                    {
                        rewardTable.Rewards.Add(reward);
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
        public int Probability; // 10000분율
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
                DataManager.ItemDict.TryGetValue(reward.ItemTemplateId, out reward.Item);
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
        // 즉발 vs 주기적 vs 영구적.
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
        public float Range;
        public float Speed;
        public float Count;
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
        // 애니메이션 모션 기다리기 위한 딜레이.
        public float DelayTime; // EventTime

        // 투사체를 날릴 경우.
        public int ProjectileId;

        // 누구한테 시전?
        public EUseSkillTargetType UseSkillTargetType;

        // 효과 대상 범위는? (0이면 단일 스킬)
        public int GatherTargetRange;
        public string GatherTargetPrefabName; // AoE

        // 피아식별
        public ETargetFriendType TargetFriendType;

        // 어떤 효과를?
        public int EffectDataId;

        // 다음 레벨 스킬.
        public int NextLevelSkillId;

        [ExcludeField]
        public SkillData NextLevelSkill;
        [ExcludeField]
        public ProjectileData ProjectileData;
        [ExcludeField]
        public EffectData EffectData;
        [ExcludeField]
        public bool IsSingleTarget;
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
                DataManager.SkillDict.TryGetValue(skill.NextLevelSkillId, out skill.NextLevelSkill);
                DataManager.ProjectileDict.TryGetValue(skill.ProjectileId, out skill.ProjectileData);
                DataManager.EffectDict.TryGetValue(skill.EffectDataId, out skill.EffectData);
                skill.IsSingleTarget = skill.GatherTargetRange == 0;
            }
            return validate;
        }
    }

    #endregion

    #region Respawn
    [Serializable]
    public class RespawnData
    {
        public int TemplateId;
        public int Count;
        public int MonsterDataId;
        public ERespawnType RespawnType;
        public int RespawnTime;
        public int PivotPosX;
        public int PivotPosY;
        public int SpawnRange;
    }

    public class RespawnDataLoader : ILoader<int, RespawnData>
    {
        public List<RespawnData> respawnDatas = new List<RespawnData>();

        public Dictionary<int, RespawnData> MakeDict()
        {
            Dictionary<int, RespawnData> dict = new Dictionary<int, RespawnData>();
            foreach (RespawnData respawnData in respawnDatas)
            {
                dict.Add(respawnData.TemplateId, respawnData);
            }
            return dict;
        }

        public bool Validate()
        {
            return true;
        }
    }
    #endregion

    #region SpawningPool

    [Serializable]
    public class SpawningPoolData
    {
        public int RoomId;
        public List<RespawnData> RespawnDatas;
    }

    [Serializable]
    public class SpawningPoolDataLoader : ILoader<int, SpawningPoolData>
    {
        public List<SpawningPoolData> spawningPools = new List<SpawningPoolData>();

        public Dictionary<int, SpawningPoolData> MakeDict()
        {
            Dictionary<int, SpawningPoolData> dict = new Dictionary<int, SpawningPoolData>();
            foreach (SpawningPoolData spawningPool in spawningPools)
            {
                dict.Add(spawningPool.RoomId, spawningPool);
            }
            return dict;
        }

        public bool Validate()
        {
            return true;
        }
    }

    #endregion

    #region Room
    public class RoomData
    {
        public int TemplateId;
        public string PrefabName;
        public string MapNameTextId;
        public string MapName;
        public int StartPosX;
        public int StartPosY;

        public SpawningPoolData SpawningPoolData;
        public List<NpcData> Npcs;
        public List<MonsterData> Monsters = new List<MonsterData>();
        public List<ItemData> DropItems = new List<ItemData>();

        #region Helpers
        public List<MonsterData> FindMonstersByDropItem(int itemId)
        {
            List<MonsterData> monsters = new List<MonsterData>();
            foreach (var monster in Monsters)
            {
                foreach (var rewardData in monster.RewardTable.Rewards)
                {
                    if (rewardData.ItemTemplateId == itemId)
                        monsters.Add(monster);
                }
            }

            return monsters;
        }

        public PositionInfo FindDropItemPosition(int itemId)
        {
            List<MonsterData> monsters = FindMonstersByDropItem(itemId);
            List<PositionInfo> positions = new List<PositionInfo>();
            if (monsters.Count == 0)
                return null;

            foreach (var monster in monsters)
            {
                positions.Add(FindMonsterSpawnPos(monster.TemplateId));
            }

            if (positions.Count == 0)
                return null;

            positions.Shuffle();

            return positions.FirstOrDefault();
        }

        public PositionInfo FindMonsterSpawnPos(int monsterId)
        {
            var respawnData = SpawningPoolData.RespawnDatas.FirstOrDefault(data => data.MonsterDataId == monsterId);

            if (respawnData == null)
                return null;

            return new PositionInfo
            {
                PosX = respawnData.PivotPosX,
                PosY = respawnData.PivotPosY,
                RoomId = TemplateId
            };
        }

        public PositionInfo FindNpcPos(int npcId)
        {
            var npcData = Npcs.FirstOrDefault(data => data.TemplateId == npcId);

            if (npcData == null)
                return null;

            return npcData.GetSpawnPosition();
        }
        #endregion
    }

    [Serializable]
    public class RoomDataLoader : ILoader<int, RoomData>
    {
        public List<RoomData> roomDatas = new List<RoomData>();

        public Dictionary<int, RoomData> MakeDict()
        {
            Dictionary<int, RoomData> dict = new Dictionary<int, RoomData>();
            foreach (RoomData spawningPool in roomDatas)
            {
                dict.Add(spawningPool.TemplateId, spawningPool);
            }
            return dict;
        }

        public bool Validate()
        {
            foreach (var roomData in roomDatas)
            {
                if (roomData.SpawningPoolData == null)
                    continue;

                // 1. Monster
                foreach (var respawnData in roomData.SpawningPoolData.RespawnDatas)
                {
                    if (DataManager.MonsterDict.TryGetValue(respawnData.MonsterDataId, out var monsterData))
                    {
                        roomData.Monsters.Add(monsterData);
                    }
                }

                // 2.Items 
                foreach (var monsterData in roomData.Monsters)
                {
                    foreach (var rewardData in monsterData.RewardTable.Rewards)
                    {
                        roomData.DropItems.Add(rewardData.Item);
                    }
                }
            }
            return true;
        }
    }

    #endregion

    #region Portal
    public class PortalData : NpcData
    {
        public int DestPotalId;
        [ExcludeField]
        public PortalData DestPortal;
    }

    [Serializable]
    public class PortalDataLoader : ILoader<int, PortalData>
    {
        public List<PortalData> portals = new List<PortalData>();

        public Dictionary<int, PortalData> MakeDict()
        {
            Dictionary<int, PortalData> dict = new Dictionary<int, PortalData>();
            foreach (PortalData portal in portals)
            {
                dict.Add(portal.TemplateId, portal);
            }
            return dict;
        }

        public bool Validate()
        {
            foreach (PortalData portal in portals)
            {
                if (DataManager.PortalDict.TryGetValue(portal.DestPotalId, out PortalData portalData))
                    portal.DestPortal = portalData;
            }
            return true;
        }
    }
    #endregion

    #region NpcCommon
    public class NpcCommonData : NpcData
    { }

    [Serializable]
    public class NpcCommonDataLoader : ILoader<int, NpcCommonData>
    {
        public List<NpcCommonData> commons = new List<NpcCommonData>();

        public Dictionary<int, NpcCommonData> MakeDict()
        {
            Dictionary<int, NpcCommonData> dict = new Dictionary<int, NpcCommonData>();
            foreach (NpcCommonData portal in commons)
            {
                dict.Add(portal.TemplateId, portal);
            }
            return dict;
        }

        public bool Validate()
        {
            return true;
        }
    }
    #endregion

    #region Quest
    [Serializable]
    public class QuestData
    {
        public int TemplateId;
        public string NameTextId;
        public EQuestType Type;
        public List<int> TaskIds;
        public int Level;
        public int RewardTableId;
        public int RequiredQuestId;
        public int OwnerRoomId;

        [ExcludeField]
        public List<QuestTaskData> QuestTasks = new List<QuestTaskData>();
        [ExcludeField]
        public RewardTableData RewardTableData = new RewardTableData();
        [ExcludeField]
        public RoomData OwnerRoomData = new RoomData();
    }

    public class QuestDataLoader : ILoader<int, QuestData>
    {
        public List<QuestData> quests = new List<QuestData>();

        public Dictionary<int, QuestData> MakeDict()
        {
            Dictionary<int, QuestData> dict = new Dictionary<int, QuestData>();
            foreach (QuestData questData in quests)
                dict.Add(questData.TemplateId, questData);

            return dict;
        }

        public bool Validate()
        {
            bool validate = true;

            //QuestTasks
            foreach (var questData in quests)
            {
                foreach (var taskId in questData.TaskIds)
                {
                    if (DataManager.QuestTaskDict.TryGetValue(taskId, out QuestTaskData questTaskData) == false)
                    {
                        validate = false;
                        continue;
                    }
                    questData.QuestTasks.Add(questTaskData);
                }

                if (DataManager.RewardTableDict.TryGetValue(questData.RewardTableId, out questData.RewardTableData) == false)
                {
                    validate = false;
                }

                if (DataManager.RoomDict.TryGetValue(questData.OwnerRoomId, out questData.OwnerRoomData) == false)
                {
                    validate = false;
                }
            }
            return validate;
        }
    }

    public class QuestTaskData
    {
        public int TemplateId;
        public string DescriptionTextId;
        public EQuestTaskType TaskType;

        public List<int> ObjectiveDataIds;
        public List<int> ObjectiveCounts;
        public int DialogueId;
        public List<int> ItemToRemoveIds;//삭제시켜야할 아이템

        [ExcludeField]
        public PositionInfo TeleportPos;
        [ExcludeField]
        public Dictionary</*목표 templateId*/int, /*Count*/int> Objectives = new Dictionary<int, int>();
    }

    public class QuestTaskDataLoader : ILoader<int, QuestTaskData>
    {
        public List<QuestTaskData> tasks = new List<QuestTaskData>();

        public Dictionary<int, QuestTaskData> MakeDict()
        {
            Dictionary<int, QuestTaskData> dict = new Dictionary<int, QuestTaskData>();
            foreach (QuestTaskData questData in tasks)
                dict.Add(questData.TemplateId, questData);

            return dict;
        }

        public bool Validate()
        {
            bool validate = true;

            foreach (var task in tasks)
            {

                for (int i = 0; i < task.ObjectiveDataIds.Count; i++)
                {
                    task.Objectives.Add(task.ObjectiveDataIds[i], task.ObjectiveCounts[i]);
                }
            }
            return validate;
        }
    }


    #endregion

    #region Collection
    [Serializable]
    public class CollectionData : BaseData
    {
        public ECollectionType Type;
        public EItemGrade Grade;
        public List<int> ItemIds;
        public int RewardEffectId;

        [ExcludeField]
        public List<ItemData> ItemDatas = new List<ItemData>();

        [ExcludeField]
        public EffectData RewardEffectData;
    }

    public class CollectionDataLoader : ILoader<int, CollectionData>
    {
        public List<CollectionData> collections = new List<CollectionData>();

        public Dictionary<int, CollectionData> MakeDict()
        {
            Dictionary<int, CollectionData> dict = new Dictionary<int, CollectionData>();
            foreach (CollectionData questData in collections)
                dict.Add(questData.TemplateId, questData);

            return dict;
        }

        public bool Validate()
        {
            bool validate = true;

            // Collections
            foreach (var collectionData in collections)
            {
                // 1. Item
                foreach (var itemId in collectionData.ItemIds)
                {
                    if (DataManager.ItemDict.TryGetValue(itemId, out ItemData item) == false)
                    {
                        validate = false;
                        continue;
                    }

                    collectionData.ItemDatas.Add(item);
                }

                // 2. RewardEffect
                if (DataManager.EffectDict.TryGetValue(collectionData.RewardEffectId, out EffectData rewardEffectData) == false)
                {
                    validate = false;
                    continue;
                }

                collectionData.RewardEffectData = rewardEffectData;
            }
            return validate;
        }

    }
    #endregion
}
