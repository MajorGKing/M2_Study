using GameServer;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    public static class Extensions
    {
        public static bool SaveChangesEx(this GameDbContext db)
        {
            try
            {
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValid(this BaseObject bc)
        {
            if (bc == null)
                return false;

            if (bc.Room == null)
                return false;

            switch (bc.ObjectType)
            {
                case EGameObjectType.Monster:
                case EGameObjectType.Hero:
                    return ((Creature)bc).State != EObjectState.Dead;
            }
            return true;
        }
    }
}
