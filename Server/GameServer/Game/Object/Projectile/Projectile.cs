using GameServer;
using GameServer.Game;
using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;


namespace Server.Game
{
    public class Projectile : BaseObject
    {
        public Creature Owner { get; set; }
        public Data.SkillData SkillData { get; private set; }
        public ProjectileData ProjectileData { get { return SkillData.Projectile; } }
        public float Speed { get { return ProjectileData.ProjSpeed; } }

        private Creature _target { get; set; }
        
    }
}
