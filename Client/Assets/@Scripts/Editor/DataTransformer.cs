using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEngine;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using Data;
using System.ComponentModel;
using System.Reflection;
using Data.SO;
using Google.Protobuf.Protocol;


public class DataTransformer : EditorWindow
{
#if UNITY_EDITOR

    [MenuItem("Tools/ParseExcel _F4")]  // 추가 단축키: Control + K
    public static void ParseExcelDataToJson()
    {
        ParseExcelDataToJson<TextDataLoader, TextData>("Text");
        ParseExcelDataToJson<BaseStatDataLoader, BaseStatData>("BaseStat");
        ParseExcelDataToJson<HeroDataLoader, HeroData>("Hero");
        ParseExcelDataToJson<MonsterDataLoader, MonsterData>("Monster");
        ParseExcelDataToJson<SkillDataLoader, SkillData>("Skill");
        ParseExcelDataToJson<EffectDataLoader, EffectData>("Effect");
        ParseExcelDataToJson<ProjectileDataLoader, ProjectileData>("Projectile");
        ParseExcelDataToJson<EquipmentDataLoader, EquipmentData>("Equipment");
        ParseExcelDataToJson<ConsumableDataLoader, ConsumableData>("Consumable");
        ParseExcelDataToJson<RewardDataLoader, RewardData>("Reward");
        ParseExcelDataToJson<DropTableDataLoader, DropTableData>("DropTable");
        //
        ParseSODataToJson<SpawningPoolDataLoader, SpawningPoolData>("SpawningPool");
        Debug.Log("Complete DataTransformer");
    }

    #region Helpers
    private static void ParseExcelDataToJson<Loader, LoaderData>(string filename) where Loader : new() where LoaderData : new()
    {
        Loader loader = new Loader();
        FieldInfo field = loader.GetType().GetFields()[0];
        field.SetValue(loader, ParseExcelDataToList<LoaderData>(filename));

        string jsonStr = JsonConvert.SerializeObject(loader, Formatting.Indented);
        File.WriteAllText($"{Application.dataPath}/@Resources/Data/JsonData/{filename}Data.json", jsonStr);
        AssetDatabase.Refresh();
    }

    private static List<LoaderData> ParseExcelDataToList<LoaderData>(string filename) where LoaderData : new()
    {
        List<LoaderData> loaderDatas = new List<LoaderData>();

        string[] lines = File.ReadAllText($"{Application.dataPath}/@Resources/Data/ExcelData/{filename}Data.csv").Split("\n");

        for (int l = 1; l < lines.Length; l++)
        {
            string[] row = lines[l].Replace("\r", "").Split(',');
            if (row.Length == 0)
                continue;
            if (string.IsNullOrEmpty(row[0]))
                continue;

            //Dragon 파생클래스를 GetField하면 파생클래스 변수 -> 부모 변수로 되어 있음. 순서 변경
            LoaderData loaderData = new LoaderData();
            Type loaderDataType = typeof(LoaderData);
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var fields = GetFieldsInBase(loaderDataType, bindingFlags);

            for (int f = 0; f < fields.Count; f++)
            {
                FieldInfo field = loaderData.GetType().GetField(fields[f].Name);
                Type type = field.FieldType;

                if (type.IsGenericType)
                {
                    Debug.Log($"filename = {filename} ,  {field} -> {row[f]}");
                    object value = ConvertList(row[f], type);
                    if (value != null)
                    {
                        field.SetValue(loaderData, value);
                    }
                }
                else
                {
                    Debug.Log($"filename = {filename} ,  {field} -> {row[f]}");
                    object value = ConvertValue(row[f], field.FieldType);
                    if (value != null)
                    {
                        field.SetValue(loaderData, value);
                    }
                }
            }
            loaderDatas.Add(loaderData);
        }

        return loaderDatas;
    }

    private static object ConvertValue(string value, Type type)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        TypeConverter converter = TypeDescriptor.GetConverter(type);
        return converter.ConvertFromString(value);
    }

    private static object ConvertList(string value, Type type)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        // Reflection
        Type valueType = type.GetGenericArguments()[0];
        Type genericListType = typeof(List<>).MakeGenericType(valueType);
        var genericList = Activator.CreateInstance(genericListType) as IList;

        // Parse Excel
        var list = value.Split('&').Select(x => ConvertValue(x, valueType)).ToList();

        foreach (var item in list)
            genericList.Add(item);

        return genericList;
    }

    public static List<FieldInfo> GetFieldsInBase(Type type, BindingFlags bindingFlags)
    {
        List<FieldInfo> fields = new List<FieldInfo>();
        HashSet<string> fieldNames = new HashSet<string>();//중복방지
        Stack<Type> stack = new Stack<Type>();

        while (type != null && type != typeof(object))
        {
            stack.Push(type);
            type = type.BaseType;
        }

        while (stack.Count > 0)
        {
            Type currentType = stack.Pop();
            foreach (var field in currentType.GetFields(bindingFlags))
            {
                if (fieldNames.Add(field.Name))
                {
                    fields.Add(field);
                }
            }
        }

        return fields;
    }

    #endregion

    #region SO Helpers

    public static void ParseSODataToJson<Loader, LoaderData>(string filename) where Loader : new() where LoaderData : ScriptableObject
    {
        string folderPath = "Assets/@Resources/Data/ScriptableObjectData";

        Loader loader = new Loader();
        FieldInfo field = loader.GetType().GetFields()[0];
        List<LoaderData> dataList = new List<LoaderData>();

        // 해당 폴더에서 모든 SOHeroData 에셋 로드
        string[] assetGuids = AssetDatabase.FindAssets($"t:{typeof(LoaderData).Name}", new[] { folderPath });

        foreach (string guid in assetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            LoaderData data = AssetDatabase.LoadAssetAtPath<LoaderData>(assetPath);
            if (data != null)
            {
                dataList.Add(data);
            }
        }

        field.SetValue(loader, dataList);

        string jsonStr = JsonConvert.SerializeObject(loader, Formatting.Indented);
        string jsonFilePath = $"{Application.dataPath}/@Resources/Data/JsonData/{filename}Data.json";
        File.WriteAllText(jsonFilePath, jsonStr);
        AssetDatabase.Refresh();

        Debug.Log($"Data has been exported to {jsonFilePath}");
    }
    #endregion

#endif

}