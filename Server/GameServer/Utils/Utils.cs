using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using Google.Protobuf.Protocol;

namespace GameServer
{
    public static class Utils
    {
        public static long TickCount { get { return System.Environment.TickCount64; } }

        public static IPAddress GetLocalIP()
        {
            var ipHost = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in ipHost.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }

            return IPAddress.Loopback;
        }

        public static Dictionary<EItemSubType, EItemSlotType> SubTypeToEquipTypeMap = new Dictionary<EItemSubType, EItemSlotType>()
        {
            { EItemSubType.Mainweapon,  EItemSlotType.Mainweapon },
            { EItemSubType.Subweapon,   EItemSlotType.Subweapon} ,
            { EItemSubType.Helmet,      EItemSlotType.Helmet },
            { EItemSubType.Chest,       EItemSlotType.Chest },
            { EItemSubType.Leg,         EItemSlotType.Leg },
            { EItemSubType.Shoes,       EItemSlotType.Shoes },
            { EItemSubType.Gloves,      EItemSlotType.Gloves },
            { EItemSubType.Shoulder,    EItemSlotType.Shoulder },
            { EItemSubType.Ring,        EItemSlotType.Ring },
            { EItemSubType.Amulet,      EItemSlotType.Amulet },
        };

        public static EItemSlotType GetEquipSlotType(EItemSubType subType)
        {
            if (SubTypeToEquipTypeMap.TryGetValue(subType, out EItemSlotType value))
                return value;

            return EItemSlotType.None;
        }

        public static T RandomElementByWeight<T>(this IEnumerable<T> sequence, Func<T, float> weightSelector)
        {
            float totalWeight = sequence.Sum(weightSelector);
            double itemWeightIndex = new Random().NextDouble() * totalWeight;
            float currentWeightIndex = 0;

            foreach (var item in from weightedItem in sequence select new { Value = weightedItem, Weight = weightSelector(weightedItem) })
            {
                currentWeightIndex += item.Weight;

                if (currentWeightIndex >= itemWeightIndex)
                    return item.Value;

            }
            return default(T);
        }
    }
}
