using System;
using System.Collections.Generic;
using System.Reflection;
using Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

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

    public Dictionary<string, TextData> TextDict { get; private set; } = new Dictionary<string, TextData>();
    public Dictionary<int, BaseStatData> BaseStatDict { get; private set; } = new Dictionary<int, BaseStatData>();
    public Dictionary<int, HeroData> HeroDict { get; private set; } = new Dictionary<int, HeroData>();
    //public Dictionary<int, QuestData> QuestDic { get; private set; } = new Dictionary<int, QuestData>();
    public Dictionary<int, SkillData> SkillDict { get; private set; } = new Dictionary<int, SkillData>();
    public Dictionary<int, EffectData> EffectDict { get; private set; } = new Dictionary<int, EffectData>();
    public Dictionary<int, MonsterData> MonsterDict { get; private set; } = new Dictionary<int, MonsterData>();
    public Dictionary<int, ProjectileData> ProjectileDict { get; private set; } = new Dictionary<int, ProjectileData>();
    public Dictionary<int, RewardData> RewardDict { get; private set; } = new Dictionary<int, RewardData>();
    public Dictionary<int, DropTableData> DropTableDict { get; private set; } = new Dictionary<int, DropTableData>();

    public Dictionary<int, ItemData> ItemDict { get; private set; } = new Dictionary<int, ItemData>();
    public Dictionary<int, EquipmentData> EquipmentDict { get; private set; } = new Dictionary<int, EquipmentData>();
    public Dictionary<int, ConsumableData> ConsumableDict { get; private set; } = new Dictionary<int, ConsumableData>();

    public void Init()
    {
        TextDict = LoadJson<TextDataLoader, string, TextData>("TextData").MakeDict();
        BaseStatDict = LoadJson<BaseStatDataLoader, int, BaseStatData>("BaseStatData").MakeDict();
        HeroDict = LoadJson<HeroDataLoader, int, HeroData>("HeroData").MakeDict();
        MonsterDict = LoadJson<MonsterDataLoader, int, MonsterData>("MonsterData").MakeDict();
        EffectDict = LoadJson<EffectDataLoader, int, EffectData>("EffectData").MakeDict();
        SkillDict = LoadJson<SkillDataLoader, int, SkillData>("SkillData").MakeDict();
        SkillDict = LoadJson<SkillDataLoader, int, SkillData>("SkillData").MakeDict();
        ProjectileDict = LoadJson<ProjectileDataLoader, int, ProjectileData>("ProjectileData").MakeDict();
        RewardDict = LoadJson<RewardDataLoader, int, RewardData>("RewardData").MakeDict();
        DropTableDict = LoadJson<DropTableDataLoader, int, DropTableData>("DropTableData").MakeDict();

        #region ItemData
        EquipmentDict = LoadJson<EquipmentDataLoader, int, EquipmentData>("EquipmentData").MakeDict();
        ConsumableDict = LoadJson<ConsumableDataLoader, int, ConsumableData>("ConsumableData").MakeDict();

        ItemDict.Clear();

        foreach (var item in EquipmentDict)
            ItemDict.Add(item.Key, item.Value);

        foreach (var item in ConsumableDict)
            ItemDict.Add(item.Key, item.Value);
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