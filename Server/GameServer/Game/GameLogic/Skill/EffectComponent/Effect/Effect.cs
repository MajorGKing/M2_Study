using GameServer;
using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Server.Data;

namespace GameServer.Game
{
    public class Effect
    {
        public int EffectId { get; private set; }
        public EffectData EffectData { get; private set; }
        public EEffectType EffectType { get { return EffectData.EffectType; } }
        public Creature Owner { get; private set; }
        public Creature Caster { get; private set; }
        
    }
}
