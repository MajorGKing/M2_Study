using System.Collections.Generic;
using Data;
using Data.SO;
using Newtonsoft.Json;
using UnityEngine;

public interface IValidate
{
    bool Validate();
}

public interface ILoader<Key, Value> : IValidate
{
    Dictionary<Key, Value> MakeDict();
}

public class DataManager
{
    private HashSet<IValidate> _loaders = new HashSet<IValidate>();

    public Dictionary<int, ConfigData> ConfigDict { get; private set; } = new Dictionary<int, ConfigData>();
    public Dictionary<string, TextData> TextDict { get; private set; } = new Dictionary<string, TextData>();
    public Dictionary<int, BaseStatData> BaseStatDict { get; private set; } = new Dictionary<int, BaseStatData>();
    public Dictionary<int, HeroData> HeroDict { get; private set; } = new Dictionary<int, HeroData>();
    //public Dictionary<int, QuestData> QuestDic { get; private set; } = new Dictionary<int, QuestData>();
    public Dictionary<int, SkillData> SkillDict { get; private set; } = new Dictionary<int, SkillData>();
    public Dictionary<int, EffectData> EffectDict { get; private set; } = new Dictionary<int, EffectData>();
    public Dictionary<int, MonsterData> MonsterDict { get; private set; } = new Dictionary<int, MonsterData>();
    public Dictionary<int, ProjectileData> ProjectileDict { get; private set; } = new Dictionary<int, ProjectileData>();
    public Dictionary<int, RewardData> RewardDict { get; private set; } = new Dictionary<int, RewardData>();
    public Dictionary<int, RewardTableData> RewardTableDict { get; private set; } = new Dictionary<int, RewardTableData>();
    public Dictionary<int, RespawnData> RespawnDict { get; private set; } = new Dictionary<int, RespawnData>();
    public Dictionary<int, SpawningPoolData> SpawningPoolDict { get; private set; } = new Dictionary<int, SpawningPoolData>();
    public Dictionary<int, RoomData> RoomDict { get; private set; } = new Dictionary<int, RoomData>();


    public Dictionary<int, NpcData> NpcDict { get; private set; } = new Dictionary<int, NpcData>();
    public Dictionary<int, PortalData> PortalDict { get; private set; } = new Dictionary<int, PortalData>();

    public Dictionary<int, ItemData> ItemDict { get; private set; } = new Dictionary<int, ItemData>();
    public Dictionary<int, EquipmentData> EquipmentDict { get; private set; } = new Dictionary<int, EquipmentData>();
    public Dictionary<int, ConsumableData> ConsumableDict { get; private set; } = new Dictionary<int, ConsumableData>();

    public void Init()
    {
        ConfigDict = LoadJson<ConfigDataLoader, int, ConfigData>("ConfigData").MakeDict();
        TextDict = LoadJson<TextDataLoader, string, TextData>("TextData").MakeDict();
        BaseStatDict = LoadJson<BaseStatDataLoader, int, BaseStatData>("BaseStatData").MakeDict();
        HeroDict = LoadJson<HeroDataLoader, int, HeroData>("HeroData").MakeDict();
        MonsterDict = LoadJson<MonsterDataLoader, int, MonsterData>("MonsterData").MakeDict();
        EffectDict = LoadJson<EffectDataLoader, int, EffectData>("EffectData").MakeDict();
        SkillDict = LoadJson<SkillDataLoader, int, SkillData>("SkillData").MakeDict();
        SkillDict = LoadJson<SkillDataLoader, int, SkillData>("SkillData").MakeDict();
        ProjectileDict = LoadJson<ProjectileDataLoader, int, ProjectileData>("ProjectileData").MakeDict();
        RewardDict = LoadJson<RewardDataLoader, int, RewardData>("RewardData").MakeDict();
        RewardTableDict = LoadJson<RewardTableDataLoader, int, RewardTableData>("RewardTableData").MakeDict();
        RespawnDict = LoadJson<RespawnDataLoader, int, RespawnData>("RespawnData").MakeDict();
        SpawningPoolDict = LoadJson<SpawningPoolDataLoader, int, SpawningPoolData>("SpawningPoolData").MakeDict();
        RoomDict = LoadJson<RoomDataLoader, int, RoomData>("RoomData").MakeDict();

        #region ItemData
        EquipmentDict = LoadJson<EquipmentDataLoader, int, EquipmentData>("EquipmentData").MakeDict();
        ConsumableDict = LoadJson<ConsumableDataLoader, int, ConsumableData>("ConsumableData").MakeDict();

        ItemDict.Clear();

        foreach (var item in EquipmentDict)
            ItemDict.Add(item.Key, item.Value);

        foreach (var item in ConsumableDict)
            ItemDict.Add(item.Key, item.Value);
        #endregion

        #region NpcData
        PortalDict = LoadJson<PortalDataLoader, int, PortalData>("PortalData").MakeDict();

        NpcDict.Clear();
        foreach (var portal in PortalDict)
        {
            NpcDict.Add(portal.Key, portal.Value);
        }
        #endregion

        Validate();
    }

    private Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        TextAsset textAsset = Managers.Resource.Load<TextAsset>($"{path}");        
        Debug.Log("Path : " + path);

        Loader loader = JsonConvert.DeserializeObject<Loader>(textAsset.text);
        _loaders.Add(loader);
        return loader;
    }

    private bool Validate()
    {
        bool success = true;

        foreach (var loader in _loaders)
        {
            if (loader.Validate() == false)
                success = false;
        }

        _loaders.Clear();

        return success;
    }
}