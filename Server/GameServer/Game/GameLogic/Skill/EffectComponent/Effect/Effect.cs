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
        public IEffectPolicy Policy { get; private set; }

        protected IJob _job;

        public Effect(int effectId, int templateId, Creature owner, Creature caster, IEffectPolicy policy)
        {
            EffectId = effectId;
            DataManager.EffectDict.TryGetValue(templateId, out EffectData effectData);
            EffectData = effectData;
            Owner = owner;
            Caster = caster;
            Policy = policy;
        }

        public virtual void Update()
        {
        }

        public virtual void Apply()
        {
            Policy?.Apply(Owner, Caster, EffectData);
        }

        public virtual void Revert()
        {
            Policy?.Revert(Owner, EffectData);

            // Job취소
            if (_job != null)
            {
                _job.Cancel = true;
                _job = null;
            }
        }
    }
}

