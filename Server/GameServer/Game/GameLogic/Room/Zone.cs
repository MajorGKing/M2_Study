using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class Zone
    {
        public int IndexX { get; private set; }
        public int IndexY { get; private set; }

        public HashSet<Hero> Heroes { get; set; } = new HashSet<Hero>();
        public HashSet<Monster> Monsters { get; set; } = new HashSet<Monster>();

        public Zone(int x, int y)
        {
            IndexX = x;
            IndexY = y;
        }

        public void Remove(BaseObject obj)
        {
            EGameObjectType type = ObjectManager.GetObjectTypeFromId(obj.ObjectId);

            switch (type)
            {
                case EGameObjectType.Hero:
                    Heroes.Remove((Hero)obj);
                    break;
            }
        }
    }
}
