using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Server.Game;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace GameServer
{
	// 게임 로직에서 완료 콜백을 받을 필요 없는 경우
	public partial class DBManager : JobSerializer
	{
		public static void SaveHeroDbNoti(Hero hero)
		{
			if (hero == null)
				return;

            // DBThread
            Push(hero.HeroDbId, () =>
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

            // DBThread
            Push(hero.HeroDbId, () =>
            {
				using(GameDbContext db = new GameDbContext())
				{
					db.Entry(itemDb).State = EntityState.Unchanged;
					db.Entry(itemDb).Property(nameof(ItemDb.EquipSlot)).IsModified = true;

					bool success = db.SaveChangesEx();
					if(!success)
					{
                        // 실패했으면 Kick
                    }
                }
			});
        }

        public static void DeleteItemNoti(Hero hero, Item item)
        {
            if (hero == null || item == null)
                return;

            if (hero.Inven.GetItemByDbId(item.Info.ItemDbId) == null)
                return;

            // 선적용.
            hero.Inven.Remove(item, sendToClient: true);

            ItemDb itemDb = new ItemDb
            {
                ItemDbId = item.Info.ItemDbId,
            };

            // DBThread
            Push(hero.HeroDbId, () =>
            {
                using (GameDbContext db = new GameDbContext())
                {
                    db.Entry(itemDb).State = EntityState.Deleted;

                    bool success = db.SaveChangesEx();
                    if (success == false)
                    {
                        // 실패했으면 Kick
                    }
                }
            });
        }

        public static void UseItemNoti(Hero hero, Item item, int useCount = 1)
        {
            if (hero == null || item == null || hero.Room == null || hero.Inven == null)
                return;
            if (item.Count <= 0)
                return;
            if (hero.Inven.GetInventoryItemByDbId(item.Info.ItemDbId) == null)
                return;
            Consumable consumable = item as Consumable;
            if (consumable == null)
                return;

            // 1. 메모리 선적용.
            consumable.UseItem(hero, useCount, sendToClient: true);

            // 2. DB 적용을 위해 세팅
            ItemDb itemDb = new ItemDb
            {
                ItemDbId = item.Info.ItemDbId,
                Count = consumable.Count
            };

            // DBThread
            Push(hero.HeroDbId, () =>
            {
                using (GameDbContext db = new GameDbContext())
                {
                    if (itemDb.Count == 0)
                    {
                        db.Items.Remove(itemDb);
                    }
                    else
                    {
                        db.Entry(itemDb).State = EntityState.Unchanged;
                        db.Entry(itemDb).Property(nameof(ItemDb.Count)).IsModified = true;
                    }

                    bool success = db.SaveChangesEx();
                    if (success == false)
                    {
                        // 실패했으면 Kick
                    }
                }
            });
        }
    }
}
