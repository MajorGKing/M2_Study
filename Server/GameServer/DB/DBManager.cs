using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Google.Protobuf.Protocol;

namespace GameServer
{
    // 게임 로직에서 완료 콜백을 받아 이어서 처리하는 경우
    public partial class DBManager : JobSerializer
    {
        public static DBManager Instance { get; } = new DBManager();

        public static List<HeroDb> LoadHeroDb(long accountDbId)
        {
            using(GameDbContext db = new GameDbContext())
            {
                List<HeroDb> heroDbs = db.Heroes.Where(h => h.AccountDbId == accountDbId).ToList();
                return heroDbs;
            }
        }

        public static HeroDb CreateHeroDb(long accountDbId, C_CreateHeroReq reqPacket)
        {
            using(GameDbContext db = new GameDbContext())
            {
                HeroDb heroDb = db.Heroes.Where(h => h.Name == reqPacket.Name).FirstOrDefault();
                if (heroDb != null)
                    return null;

                heroDb = new HeroDb()
                {
                    AccountDbId = accountDbId,
                    Name = reqPacket.Name,
                    Gender = reqPacket.Gender,
                    ClassType = reqPacket.ClassType,
                    Level = 1,
                    Hp = -1,
                    Mp = -1,
                };

                db.Heroes.Add(heroDb);

                if (db.SaveChangesEx())
                    return heroDb;

                return null;
            }
        }

        public static bool DeleteHeroDb(int heroDbId)
        {
            using (GameDbContext db = new GameDbContext())
            {
                HeroDb heroDb = db.Heroes.Where(h => h.HeroDbId == heroDbId).FirstOrDefault();
                if (heroDb == null)
                    return false;

                db.Heroes.Remove(heroDb);

                if (db.SaveChangesEx())
                    return true;
            }

            return true;
        }
    }
}
