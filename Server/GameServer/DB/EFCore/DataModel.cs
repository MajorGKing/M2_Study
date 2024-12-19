using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

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
        public int Exp { get; set; }
        public int MapId { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }
    }

    [Table("Item")]
    public class ItemDb
    {
        // Convention : [클래스]Id 으로 명명하면 PK
        public long ItemDbId { get; set; }
        public int TemplateId { get; set; }
        public int EquipSlot { get; set; }
        public long AccountDbId { get; set; }
        public int Count { get; set; }
        public int OwnerDbId { get; set; }
        public int EnchantCount { get; set; }
    }
}
