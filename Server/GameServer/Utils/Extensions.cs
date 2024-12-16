using GameServer;
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
    }
}
