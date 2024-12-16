using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace GameServer
{
    // 게임 로직에서 완료 콜백을 받아 이어서 처리하는 경우
    public partial class DBManager : JobSerializer
    {
        public static DBManager Instance { get; } = new DBManager();

        /*
		// Me (GameRoom) -> You (Db) -> Me (GameRoom)
		public static void SavePlayerStatus_AllInOne(Player player, GameRoom room)
		{
			if (player == null || room == null)
				return;

			// Me (GameRoom)
			PlayerDb playerDb = new PlayerDb();
			playerDb.PlayerDbId = player.PlayerDbId;
			playerDb.Hp = player.Stat.Hp;

			// You
			Instance.Push(() =>
			{
				using (AppDbContext db = new AppDbContext())
				{
					db.Entry(playerDb).State = EntityState.Unchanged;
					db.Entry(playerDb).Property(nameof(PlayerDb.Hp)).IsModified = true;
					bool success = db.SaveChangesEx();
					if (success)
					{
						// Me
					}
				}
			});
		}

		// Me (GameRoom)
		public static void SavePlayerStatus_Step1(Player player, GameRoom room)
		{
			if (player == null || room == null)
				return;

			// Me (GameRoom)
			PlayerDb playerDb = new PlayerDb();
			playerDb.PlayerDbId = player.PlayerDbId;
			playerDb.Hp = player.Stat.Hp;
			Instance.Push<PlayerDb, GameRoom>(SavePlayerStatus_Step2, playerDb, room);
		}

		// You (Db)
		public static void SavePlayerStatus_Step2(PlayerDb playerDb, GameRoom room)
		{
			using (AppDbContext db = new AppDbContext())
			{
				db.Entry(playerDb).State = EntityState.Unchanged;
				db.Entry(playerDb).Property(nameof(PlayerDb.Hp)).IsModified = true;
				bool success = db.SaveChangesEx();
				if (success)
				{
					room.Push(SavePlayerStatus_Step3, playerDb.Hp);
				}
			}
		}

		// Me
		public static void SavePlayerStatus_Step3(int hp)
		{

		}

		public static void RewardPlayer(Player player, RewardData rewardData, GameRoom room)
		{
			if (player == null || rewardData == null || room == null)
				return;

			int? slot = player.Inven.GetEmptySlot();
			if (slot == null)
				return;

			ItemDb itemDb = new ItemDb()
			{
				TemplateId = rewardData.itemId,
				Count = rewardData.count,
				Slot = slot.Value,
				OwnerDbId = player.PlayerDbId
			};

			// You
			Instance.Push(() =>
			{
				using (AppDbContext db = new AppDbContext())
				{
					db.Items.Add(itemDb);
					bool success = db.SaveChangesEx();
					if (success)
					{
						// Me
						room.Push(() =>
						{
							Item newItem = Item.MakeItem(itemDb);
							player.Inven.Add(newItem);

							// Client Noti
							{
								S_AddItem itemPacket = new S_AddItem();
								ItemInfo itemInfo = new ItemInfo();
								itemInfo.MergeFrom(newItem.Info);
								itemPacket.Items.Add(itemInfo);

								player.Session.Send(itemPacket);
							}
						});
					}
				}
			});
		}
		*/
    }
}
