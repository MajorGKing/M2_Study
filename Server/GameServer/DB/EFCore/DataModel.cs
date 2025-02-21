using Google.Protobuf.Protocol;
using GameServer.Game;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GameServer
{
    [Table("Hero")]
    public class HeroDb
    {
        // Convention : [클래스]Id 으로 명명하면 PK
        public int HeroDbId { get; set; }
        public long AccountDbId { get; set; }
        public DateTime CreateDate { get; private set; }
        public EHeroGender Gender { get; set; }
        public EHeroClass ClassType { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int Hp { get; set; }
        public int Mp { get; set; }
        public int Exp { get; set; }
        public int MapId { get; set; } //TODO RoomId 로 바꾸기
        public int? PosX { get; set; }
        public int? PosY { get; set; }
        public int Gold { get; set; }
        public int Dia { get; set; }
        public ICollection<ItemDb> Items { get; set; } = new List<ItemDb>();
        public ICollection<QuestDb> Quests { get; set; } = new List<QuestDb>();
    }

    [Table("Item")]
    public class ItemDb
    {
        // Convention : [클래스]Id 으로 명명하면 PK
        public long ItemDbId { get; set; }
        public int TemplateId { get; set; }
        public EItemSlotType EquipSlot { get; set; }
        public int Count { get; set; }
		
        // FK
        public int OwnerDbId { get; set; }
        public HeroDb OwnerDb { get; set; }
    }

    [Table("Quest")]
    [PrimaryKey(nameof(OwnerDbId), nameof(TemplateId))]
    public class QuestDb
    {
        // PK, FK
        public int OwnerDbId { get; set; }
        public int TemplateId { get; set; }
        public EQuestState State { get; set; }

        // JSON
        public List<QuestTaskDb> QuestTasks { get; set; } = new List<QuestTaskDb>();

        // FK        
        public HeroDb OwnerDb { get; set; }
    }

    [Owned]
    public class QuestTaskDb
    {
        public List<int> ObjectiveTemplateIds { get; set; } = new List<int>();
        public List<int> ObjectiveCounts { get; set; } = new List<int>();
    }
}
