using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data.SO
{
    [CreateAssetMenu(fileName = "Assets/@Resources/Data/ScriptableObjectData/Config/FILENAME", menuName = "Scriptable Objects/Config", order = 0)]
    public class ConfigData : ScriptableObject
    {
        public int TemplateId;
        public string ServerName;
        public string ServerIp;
        public int ServerPort;
    }

    [Serializable]
    public class ConfigDataLoader : ILoader<int, ConfigData>
    {
        public List<ConfigData> configs = new List<ConfigData>();

        public Dictionary<int, ConfigData> MakeDict()
        {
            Dictionary<int, ConfigData> dict = new Dictionary<int, ConfigData>();
            foreach (ConfigData config in configs)
            {
                dict.Add(config.TemplateId, config);
            }
            return dict;
        }

        public bool Validate()
        {
            return true;
        }
    }
}