using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using Scripts.Data.SO;
using UnityEngine;

namespace Data.SO
{
    [CreateAssetMenu(fileName = "Assets/@Resources/Data/ScriptableObjectData/NPC/Common/FILENAME", menuName = "Scriptable Objects/NPC/Common", order = 0)]
    public class NpcCommonData : NpcData
    {
    }

    [Serializable]
    public class NpcCommonDataLoader : ILoader<int, NpcCommonData>
    {
        public List<NpcCommonData> commons = new List<NpcCommonData>();

        public Dictionary<int, NpcCommonData> MakeDict()
        {
            Dictionary<int, NpcCommonData> dict = new Dictionary<int, NpcCommonData>();
            foreach (NpcCommonData portal in commons)
            {
                dict.Add(portal.TemplateId, portal);
            }
            return dict;
        }

        public bool Validate()
        {
            bool valid = true;
            foreach (NpcCommonData npc in commons)
            {
                npc.SpawnPosInfo = new PositionInfo();
                npc.SpawnPosInfo.RoomId = npc.OwnerRoomId;
                npc.SpawnPosInfo.PosX = npc.SpawnPosX;
                npc.SpawnPosInfo.PosY = npc.SpawnPosY;
                
                if(Managers.Data.DialogueDict.TryGetValue(npc.DialogueId, out DialogueData dialogue) == false)
                {
                    Debug.LogError("Dialogue ID " + npc.DialogueId + " not found");
                    valid = false;
                }
            }
            return valid;
        }
    }

}