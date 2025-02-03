using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

namespace Data.SO
{
    #region SpawningPool
    [CreateAssetMenu(fileName = "Assets/@Resources/Data/ScriptableObjectData/SpawningPool/FILENAME", menuName = "Scriptable Objects/SpawningPool", order = 0)]
    public class SpawningPoolData : ScriptableObject
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
}