using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

namespace Data.SO
{
    [CreateAssetMenu(fileName = "Assets/@Resources/Data/ScriptableObjectData/NPC/Portal/FILENAME", menuName = "Scriptable Objects/NPC/Portal", order = 0)]
    public class PortalData : NpcData
    {
        [Space(10)]
        public int DestPotalId;
        [NonSerialized] public PortalData DestPortal;
    }

    [Serializable]
    public class PortalDataLoader : ILoader<int, PortalData>
    {
        public List<PortalData> portals = new List<PortalData>();

        public Dictionary<int, PortalData> MakeDict()
        {
            Dictionary<int, PortalData> dict = new Dictionary<int, PortalData>();
            foreach (PortalData portal in portals)
            {
                dict.Add(portal.TemplateId, portal);
            }
            return dict;
        }

        public bool Validate()
        {
            foreach (PortalData portal in portals)
            {
                if (Managers.Data.PortalDict.TryGetValue(portal.DestPotalId, out PortalData portalData))
                    portal.DestPortal = portalData;

                portal.SpawnPosInfo = new PositionInfo();
                portal.SpawnPosInfo.RoomId = portal.OwnerRoomId;
                portal.SpawnPosInfo.PosX = portal.SpawnPosX;
                portal.SpawnPosInfo.PosY = portal.SpawnPosY;
            }

            return true;
        }

    }
}
