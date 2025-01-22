using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Google.Protobuf.Protocol;
using Server.Data;
using System.Numerics;
using Server.Game;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Collections;

namespace GameServer
{
    // 게임 로직에서 완료 콜백을 받아 이어서 처리하는 경우
    public partial class DBManager : JobSerializer
    {
        // GameThread
        public static void UseItem(Hero hero, Item item, int useCount = 1)
        {
            if (hero == null || item == null || hero.Room == null || hero.Inven == null)
                return;
            if (item.Count < -0)
                return;
            if (hero.Inven.GetInventoryItemByDbId(item.Info.ItemDbId) == null)
                return;

            ItemDb itemDb = new ItemDb
            {
                ItemDbId = item.Info.ItemDbId,
            };

            // DB 적용을 위해 세팅.
            itemDb.Count = Math.Clamp(item.Count - useCount, 0, item.MaxStack);

            Instance.Push(UseItem_Step2, hero, itemDb, useCount);
        }

        // DBThread
        public static void UseItem_Step2(Hero hero, ItemDb itemDb, int useCount)
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
                if (success)
                {
                    hero.Room.Push(UseItem_Step3, hero, itemDb, useCount);
                }
            }
        }

        // GameThread
        public static void UseItem_Step3(Hero hero, ItemDb itemDb, int useCount)
        {
            Consumable item = hero.Inven.GetInventoryItemByDbId(itemDb.ItemDbId) as Consumable;
            if (item == null)
                return;

            item.UseItem(hero, useCount, sendToClient: true);
        }

        // GameThread
        public static void DeleteItem(Hero hero, Item item)
        {
            if(hero == null || item == null) 
                return;

            if (hero.Inven.GetItemByDbId(item.Info.ItemDbId) == null)
                return;

            ItemDb itemDb = new ItemDb
            {
                ItemDbId = item.Info.ItemDbId,
            };

            Instance.Push(DeleteItem_Step2, hero, itemDb);
        }

        // DBThread
        public static void DeleteItem_Step2(Hero hero, ItemDb itemDb)
        {
            using(GameDbContext db = new GameDbContext())
            {
                db.Entry(itemDb).State = EntityState.Deleted;

                bool success = db.SaveChangesEx();
                if(success)
                {
                    hero.Room.Push(DeleteItem_Step3, hero, itemDb);
                }
            }
        }

        // GameThread
        public static void DeleteItem_Step3(Hero hero, ItemDb itemDb)
        {
            Item item = hero.Inven.GetItemByDbId(itemDb.ItemDbId);
            if (item == null)
                return;

            hero.Inven.Remove(item, sendToClient: true);
        }
    }
}
