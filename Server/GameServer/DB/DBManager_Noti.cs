using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
	// 게임 로직에서 완료 콜백을 받을 필요 없는 경우
	public partial class DBManager : JobSerializer
	{
		public static void TestDB()
		{
			HeroDb heroDb = new HeroDb();
			heroDb.AccountDbId = new Random().Next(0, 100);

			using (GameDbContext db = new GameDbContext())
			{
				db.Add(heroDb);
				db.SaveChangesEx();
			}
		}

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
	}
}
