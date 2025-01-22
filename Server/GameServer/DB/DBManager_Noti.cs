using Microsoft.EntityFrameworkCore;
using Server.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
	// 게임 로직에서 완료 콜백을 받을 필요 없는 경우
	public partial class DBManager : JobSerializer
	{
		/*
		public static void EquipItemNoti(Player player, Item item)
		{
			if (player == null || item == null)
				return;

			ItemDb itemDb = new ItemDb()
			{
				ItemDbId = item.ItemDbId,
				Equipped = item.Equipped
			};

			// You
			Instance.Push(() =>
			{
				using (AppDbContext db = new AppDbContext())
				{
					db.Entry(itemDb).State = EntityState.Unchanged;
					db.Entry(itemDb).Property(nameof(ItemDb.Equipped)).IsModified = true;

					bool success = db.SaveChangesEx();
					if (!success)
					{
						// 실패했으면 Kick
					}
				}
			});
		}
		*/

		public static void SaveHeroDbNoti(Hero hero)
		{
			if (hero == null)
				return;

			Instance.Push(() =>
			{
				using(GameDbContext db = new GameDbContext())
				{
					HeroDb heroDb = db.Heroes.Where(h => h.HeroDbId == hero.HeroDbId).FirstOrDefault();
					if (heroDb == null)
						return;

                    heroDb.Level = hero.HeroInfo.Level;
                    heroDb.Exp = hero.MyHeroInfo.Exp;
                    heroDb.Hp = (int)hero.Hp;
                    heroDb.Mp = (int)hero.Mp;
                    heroDb.PosX = hero.PosInfo.PosX;
                    heroDb.PosY = hero.PosInfo.PosY;
                    heroDb.Gold = hero.MyHeroInfo.CurrencyInfo.Gold;
                    heroDb.Dia = hero.MyHeroInfo.CurrencyInfo.Dia;

                    bool success = db.SaveChangesEx();
					if(success == false)
					{
                        // 실패했으면 Kick
                    }
                }
			});
		}

		public static void EquipItemNoti(Hero hero, Item item)
		{
            if (hero == null || item == null)
                return;

            ItemDb itemDb = new ItemDb()
            {
                ItemDbId = item.ItemDbId,
                EquipSlot = item.ItemSlotType
            };

			// You
			Instance.Push(() =>
			{
				using(GameDbContext db = new GameDbContext())
				{
					db.Entry(itemDb).State = EntityState.Unchanged;
					db.Entry(itemDb).Property(nameof(ItemDb.EquipSlot)).IsModified = true;

					bool success = db.SaveChangesEx();
					if(!success)
					{

					}
				}
			});
        }
	}
}
