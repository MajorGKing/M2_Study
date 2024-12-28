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
    public Dictionary<int, MonsterData> MonsterDic { get; private set; } = new Dictionary<int, MonsterData>();

    public void Init()
    {
        TextDic = LoadJson<TextDataLoader, string, TextData>("TextData").MakeDict();
        MonsterDic = LoadJson<MonsterDataLoader, int, MonsterData>("MonsterData").MakeDict();
        HeroDic = LoadJson<HeroDataLoader, int, HeroData>("HeroData").MakeDict();

        //TODO ��� ���ֱ�  JsonConvert.DeserializeObject�� ���������� new�� �ϴµ� SOŬ������ ������ ���
        //SO
        SkillDic = LoadJson<SkillDataLoader, int, SkillData>("SkillData").MakeDict();
        QuestDic = LoadJson<QuestDataLoader, int, QuestData>("QuestData").MakeDict();

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