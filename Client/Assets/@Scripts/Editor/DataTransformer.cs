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
	    ParseSODataToJson<ConfigDataLoader, ConfigData>("Config");
	    ParseSODataToJson<RespawnDataLoader, RespawnData>("Respawn");
	    ParseSODataToJson<SpawningPoolDataLoader, SpawningPoolData>("SpawningPool");
	    ParseSODataToJson<RoomDataLoader, RoomData>("Room");
	    ParseSODataToJson<PortalDataLoader, PortalData>("Portal");
	    

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
        string[] lines = File.ReadAllText($"{Application.dataPath}/@Resources/Data/ExcelData/{filename}Data.csv").Trim().Split("\n");

        List<string[]> rows = new List<string[]>();

        int innerFieldCount = 0;
        for (int l = 1; l < lines.Length; l++)
        {
            string[] row = lines[l].Replace("\r", "").Split(',');
            rows.Add(row);
        }

        for (int r = 0; r < rows.Count; r++)
        {
            if (rows[r].Length == 0)
                continue;
            if (string.IsNullOrEmpty(rows[r][0]))
                continue;
            innerFieldCount = 0;
            //Dragon 파생클래스를 GetField하면 파생클래스 변수 -> 부모 변수로 되어 있음. 순서 변경
            LoaderData loaderData = new LoaderData();
            Type loaderDataType = typeof(LoaderData);
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var fields = GetFieldsInBase(loaderDataType, bindingFlags);

            int nextIndex;
            for (nextIndex = r + 1; nextIndex < rows.Count; nextIndex++)
            {
                if (string.IsNullOrEmpty(rows[nextIndex][0]) == false)
                    break;
            }

            for (int f = 0; f < fields.Count; f++)
            {
                FieldInfo field = loaderData.GetType().GetField(fields[f].Name);
                Type type = field.FieldType;

                if (type.IsGenericType)
                {
                    Type valueType = type.GetGenericArguments()[0];
                    Type genericListType = typeof(List<>).MakeGenericType(valueType);
                    var genericList = Activator.CreateInstance(genericListType) as IList;

                    for (int i = r; i < nextIndex; i++)
                    {
                        if (string.IsNullOrEmpty(rows[i][f + innerFieldCount]))
                            continue;
                        Debug.Log($"filename = {filename} ,  {field} -> {rows[i][f]}");
                        {
                            bool isCustomClass = valueType.IsClass && !valueType.IsPrimitive && valueType != typeof(string);

                            if (isCustomClass)
                            {
                                object fieldInstance = Activator.CreateInstance(valueType);

                                Type fieldType = fieldInstance.GetType();
                                FieldInfo[] fieldInfos = fieldType.GetFields(BindingFlags.Public | BindingFlags.Instance);

                                for (int k = 0; k < fieldInfos.Length; k++) 
                                {
                                    FieldInfo innerField = valueType.GetFields()[k];
                                    string str = rows[i][f + innerFieldCount + k];
                                    object convertedValue = ConvertValue(str, innerField.FieldType);
                                    if (convertedValue != null)
                                    {
                                        innerField.SetValue(fieldInstance, convertedValue);
                                    }
                                }
                                
                                string nextStr = null;
                                if (i + 1 < rows.Count) 
                                {
                                    if (f + innerFieldCount < rows[i + 1].Length)
                                    {
                                        //DataId가 null이면 리스트가 아직 끝난게 아님
                                        if(string.IsNullOrEmpty(rows[i+1][0]))
                                            nextStr = rows[i + 1][f + innerFieldCount];
                                    }
                                }                               
                                if (string.IsNullOrEmpty(nextStr))
                                {
                                    innerFieldCount = fieldInfos.Length - 1;
                                }
                                else if(i+1 == nextIndex)
                                    innerFieldCount = fieldInfos.Length - 1;
                                
                                genericList.Add(fieldInstance);

                                // field.SetValue(loaderData, fieldInstance);
                            }
                            else
                            {
                                object value = ConvertValue(rows[i][f], valueType);
                                genericList.Add(value);
                            }
                        }
                    }

                    if (genericList != null)
                    {
                        field.SetValue(loaderData, genericList);
                    }
                }
                else
                {
                    Debug.Log($"filename = {filename} ,  {field} -> {rows[r][f]}");
                    bool isCustomClass = field.FieldType.IsClass && !field.FieldType.IsPrimitive && field.FieldType != typeof(string);
                    if (isCustomClass)
                    {
                        object fieldInstance = Activator.CreateInstance(field.FieldType);

                        Type fieldType = fieldInstance.GetType();
                        FieldInfo[] fieldInfos = fieldType.GetFields(BindingFlags.Public | BindingFlags.Instance);

                        for (int i = 0; i < fieldInfos.Length; i++) 
                        {
                            FieldInfo innerField = field.FieldType.GetFields()[i];
                            string value = rows[r][f + innerFieldCount + i];
                            object convertedValue = ConvertValue(value, innerField.FieldType);
                            if (convertedValue != null)
                            {
                                innerField.SetValue(fieldInstance, convertedValue);
                            }

                        }
                        innerFieldCount = fieldInfos.Length - 1;
                        field.SetValue(loaderData, fieldInstance);
                    }
                    else
                    {
                        //기타필드 처리
                        object value = ConvertValue(rows[r][f], field.FieldType);
                        if (value != null)
                        {
                            field.SetValue(loaderData, value);
                        }
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
					if (field.GetCustomAttribute<ExcludeFieldAttribute>() != null)
						continue;
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