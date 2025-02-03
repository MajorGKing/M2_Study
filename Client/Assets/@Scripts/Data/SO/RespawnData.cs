using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

namespace Data.SO
{
    [CreateAssetMenu(fileName = "Assets/@Resources/Data/ScriptableObjectData/Respawn/FILENAME", menuName = "Scriptable Objects/Respawn", order = 0)]
    public class RespawnData : ScriptableObject
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
}
