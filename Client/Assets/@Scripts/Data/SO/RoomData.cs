using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data.SO
{
    #region Room
    [CreateAssetMenu(fileName = "Assets/@Resources/Data/ScriptableObjectData/Room/FILENAME", menuName = "Scriptable Objects/Room", order = 0)]
    public class RoomData : ScriptableObject
    {
        public int TemplateId;
        public string PrefabName;
        public string MapNameTextId;
        public string MapName;
        public int StartPosX;
        public int StartPosY;
        [Space(20)]
        public SpawningPoolData SpawningPoolData;
        public List<NpcData> Npcs;
    }

    [Serializable]
    public class RoomDataLoader : ILoader<int, RoomData>
    {
        public List<RoomData> spawningPools = new List<RoomData>();

        public Dictionary<int, RoomData> MakeDict()
        {
            Dictionary<int, RoomData> dict = new Dictionary<int, RoomData>();
            foreach (RoomData spawningPool in spawningPools)
            {
                dict.Add(spawningPool.TemplateId, spawningPool);
            }
            return dict;
        }

        public bool Validate()
        {
            return true;
        }
    }
    #endregion
}
