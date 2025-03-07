using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using Scripts.Data.SO;
using UnityEngine;

namespace Data.SO
{
    public class NpcData : ScriptableObject
    {
        public int TemplateId;
        public string Name; //개발용
        public string NameTextId;
        public string DescriptionTextID;
        public string IconImage;
        public string PrefabName;
        public ENpcType NpcType;
        public int ExtraCells;
        public int Range;
        
        public int OwnerRoomId;
        public int SpawnPosX;
        public int SpawnPosY;

        public int DialogueId;

        [ExcludeField] 
        public PositionInfo SpawnPosInfo;
        [ExcludeField] 
        public DialogueData DialogueData;
    }

    [Serializable]
    public class NpcDataLoader : ILoader<int, NpcData>
    {
        public List<NpcData> portals = new List<NpcData>();

        public Dictionary<int, NpcData> MakeDict()
        {
            Dictionary<int, NpcData> dict = new Dictionary<int, NpcData>();
            foreach (NpcData portal in portals)
            {
                dict.Add(portal.TemplateId, portal);
            }
            return dict;
        }

        public bool Validate()
        {
            return true;
        }
    }

}