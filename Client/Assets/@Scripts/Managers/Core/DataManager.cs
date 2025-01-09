using System;
using System.Collections.Generic;
using System.Reflection;
using Data;
using Scripts.Data.SO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scripts.Data;
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

    public Dictionary<string, TextData> TextDic { get; private set; } = new Dictionary<string, TextData>();
    public Dictionary<int, HeroData> HeroDic { get; private set; } = new Dictionary<int, HeroData>();
    public Dictionary<int, QuestData> QuestDic { get; private set; } = new Dictionary<int, QuestData>();
    public Dictionary<int, SkillData> SkillDic { get; private set; } = new Dictionary<int, SkillData>();
    public Dictionary<int, EffectData> EffectDic { get; private set; } = new Dictionary<int, EffectData>();
    public Dictionary<int, MonsterData> MonsterDic { get; private set; } = new Dictionary<int, MonsterData>();
    public Dictionary<int, ProjectileData> ProjectileDic { get; private set; } = new Dictionary<int, ProjectileData>();

    public void Init()
    {
        TextDic = LoadJson<TextDataLoader, string, TextData>("TextData").MakeDict();


        //TODO 경고문 없애기  JsonConvert.DeserializeObject이 내부적으로 new를 하는데 SO클래스기 때문에 경고남
        //SO
        HeroDic = LoadJson<HeroDataLoader, int, HeroData>("HeroData").MakeDict();
        MonsterDic = LoadJson<MonsterDataLoader, int, MonsterData>("MonsterData").MakeDict();
        QuestDic = LoadJson<QuestDataLoader, int, QuestData>("QuestData").MakeDict();
        SkillDic = LoadJson<SkillDataLoader, int, SkillData>("SkillData").MakeDict();
        EffectDic = LoadJson<EffectDataLoader, int, EffectData>("EffectData").MakeDict();
        ProjectileDic = LoadJson<ProjectileDataLoader, int, ProjectileData>("ProjectileData").MakeDict();



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