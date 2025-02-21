using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Game;

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

					heroDb.Level = hero.HeroInfoComp.HeroInfo.Level;
					heroDb.Exp = hero.HeroInfoComp.MyHeroInfo.Exp;
                    heroDb.Hp = (int)hero.StatComp.Hp;
                    heroDb.Mp = (int)hero.StatComp.Mp;
                    heroDb.PosX = hero.PosInfo.PosX;
					heroDb.PosY = hero.PosInfo.PosY;
					heroDb.Gold = hero.HeroInfoComp.MyHeroInfo.CurrencyInfo.Gold;
					heroDb.Dia = hero.HeroInfoComp.MyHeroInfo.CurrencyInfo.Dia;
					heroDb.MapId = hero.HeroInfoComp.MyHeroInfo.MapId;

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
                    if (success == false)
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

        public static void AddQuestNoti(Hero hero, QuestData questData)
        {
            // GameThread
            if (hero == null || questData == null)
                return;

            QuestDb questDb = new QuestDb
            {
                TemplateId = questData.TemplateId,
                State = EQuestState.Processing,
                OwnerDbId = hero.HeroDbId,
            };

            foreach (QuestTaskData questTaskData in questData.QuestTasks)
            {
                QuestTaskDb taskDb = new QuestTaskDb();

                foreach (int objectiveId in questTaskData.ObjectiveDataIds)
                {
                    taskDb.ObjectiveTemplateIds.Add(objectiveId);
                    taskDb.ObjectiveCounts.Add(0);
                }

                questDb.QuestTasks.Add(taskDb);
            }

            // 메모리 선적용.
            hero.QuestComp.AddQuestFromDb(questDb, sendToClient: true);

            Push(hero.HeroDbId, () =>
            {
                // DBThread
                using (GameDbContext db = new GameDbContext())
                {
                    db.Quests.Add(questDb);

                    bool success = db.SaveChangesEx();
                    if (success == true)
                    {
                    }
                }
            });
        }

        public static void SaveQuestNoti(Hero hero, QuestInfo questInfo)
        {
            if (hero == null)
                return;

            // DBThread
            Push(hero.HeroDbId, () =>
            {
                using (GameDbContext db = new GameDbContext())
                {
                    QuestDb questDb = db.Quests
                        .Include(q => q.QuestTasks) // QuestTasks를 명시적으로 포함하여 로드
                        .Where(q => q.OwnerDbId == hero.HeroDbId && q.TemplateId == questInfo.TemplateId)
                        .FirstOrDefault();

                    if (questDb == null)
                        return;

                    // Quest 상태 업데이트
                    questDb.State = questInfo.QuestState;
                    questDb.QuestTasks.Clear();

                    // 새로운 QuestTask 추가
                    for (int i = 0; i < questInfo.TaskInfos.Count; i++)
                    {
                        QuestTaskInfo taskInfo = questInfo.TaskInfos[i];
                        QuestTaskDb questTaskDb = new QuestTaskDb
                        {
                            ObjectiveTemplateIds = taskInfo.Objectives.Keys.ToList(),
                            ObjectiveCounts = taskInfo.Objectives.Values.ToList()
                        };
                        questDb.QuestTasks.Add(questTaskDb);
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
