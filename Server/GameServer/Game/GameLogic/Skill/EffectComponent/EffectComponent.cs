using Google.Protobuf.Protocol;
using Server.Data;

namespace GameServer.Game
{
    public class EffectComponent
    {
        public static int _effetIdGenerator = 1;

        public static int GenerateEffectId()
        {
            return Interlocked.Increment(ref _effetIdGenerator);
        }

        public static readonly Dictionary<EEffectType, IEffectPolicy> _policies = new Dictionary<EEffectType, IEffectPolicy>()
        {
            { EEffectType.Damage, new DamageEffectPolicy() },
            { EEffectType.Heal, new DummyPolicy() },
            { EEffectType.BuffStat, new BuffStatEffectPolicy() },
            { EEffectType.BuffLifeSteal, new DummyPolicy() },
            { EEffectType.BuffStun, new BuffStunPolicy() },
        };

        Dictionary<int/*buffId*/, Effect> _effects = new Dictionary<int, Effect>();
        public Creature Owner { get; private set; }

        public EffectComponent(Creature owner)
        {
            Owner = owner;
        }

        public void ApplyEffect(int templateId, Creature caster)
        {
            if(DataManager.EffectDict.TryGetValue(templateId, out EffectData effectData) == false)
                return;

            switch (effectData.DurationPolicy)
            {
                case EDurationPolicy.Instant:
                    ApplyInstantEffect(effectData, caster);
                    break;
                case EDurationPolicy.Duration:
                    ApplyDurationEffect(effectData, caster);
                    break;
                case EDurationPolicy.Infinite:
                    ApplyInfiniteEffect(effectData, caster);
                    break;
            }
        }

        void ApplyInstantEffect(EffectData effectData, Creature caster)
        {
            Console.WriteLine($"Policy Apply {effectData.EffectType}");
            if (effectData == null)
                return;

            if (_policies.TryGetValue(effectData.EffectType, out IEffectPolicy policy) == false)
                return;

            policy.Apply(Owner, caster, effectData);
        }

        void ApplyDurationEffect(EffectData effectData, Creature caster)
        {
            if (effectData == null)
                return;
            if (effectData.Duration == 0)
                return;
            if (_policies.TryGetValue(effectData.EffectType, out IEffectPolicy policy) == false)
                return;

            int effectId = GenerateEffectId();
            Effect effect = new Effect(effectId, effectData.TemplateId, Owner, caster, policy);
            _effects.Add(effectId, effect);

            effect.Apply();

            Owner.Room.PushAfter((int)(effectData.Duration * 1000), () => { RemoveEffect(effect); });
        }

        void ApplyInfiniteEffect(EffectData effectData, Creature caster)
        {
            if (effectData == null)
                return;
            if (effectData.Duration == 0)
                return;
            if (_policies.TryGetValue(effectData.EffectType, out IEffectPolicy policy) == false)
                return;

            int effectId = GenerateEffectId();
            Effect effect = new Effect(effectId, effectData.TemplateId, Owner, caster, policy);
            _effects.Add(effectId, effect);

            effect.Apply();
        }

        public void RemoveEffect(int effectId, bool send = true)
        {
            if (_effects.TryGetValue(effectId, out Effect effect))
                RemoveEffect(effect, send);
        }

        public void RemoveEffect(Effect effect, bool send = true)
        {
            _effects.Remove(effect.EffectId);

            effect.Revert();

            if (send)
                SendChangeEffectPacket();
        }

        public void Clear()
        {
            foreach(Effect effect in _effects.Values.ToList())
                RemoveEffect(effect, false);

            _effects.Clear();
            SendChangeEffectPacket();
        }

        // TODO : Rookiss 2024/08/03
        private void SendChangeEffectPacket()
        {
            S_ChangeEffects packet = new S_ChangeEffects();
            packet.ObjectId = Owner.ObjectId;

            foreach(Effect effect in _effects.Values)
                packet.EffectIds.Add(effect.EffectData.TemplateId);

            Owner.Room?.Broadcast(Owner.CellPos, packet);

            if(Owner.ObjectType == EGameObjectType.Hero)
            {
                (Owner as Hero).RefreshTotalStat(true);
            }
        }
    }
}
