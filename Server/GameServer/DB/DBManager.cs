using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
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
    }
}
