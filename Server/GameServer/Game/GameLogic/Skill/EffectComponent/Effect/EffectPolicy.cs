using Server.Data;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Game;
using System.ComponentModel;
using Google.Protobuf.WellKnownTypes;

namespace GameServer.Game
{
    public interface IEffectPolicy
    {
        void Apply(Creature owner, Creature caster, EffectData effectData);
        void Revert(Creature owner, EffectData effectData);
    }

    public class DummyPolicy : IEffectPolicy
    {
        public void Apply(Creature owner, Creature caster, EffectData effectData)
        {
        }

        public void Revert(Creature owner, EffectData effectData)
        {
        }
    }

    public class DamageEffectPolicy : IEffectPolicy
    {
        public void Apply(Creature owner, Creature caster, EffectData effectData)
        {
            if (owner == null)
                return;

            owner.Hp -= effectData.DamageValue;

            Console.WriteLine($"HP : {owner.Hp} \tDamaged : {effectData.DamageValue}");

            if(effectData.DamageValue > 0)
                owner.OnDamaged(caster, effectData.DamageValue);
        }

        public void Revert(Creature owner, EffectData effectData)
        { 
        }
    }

    public class BuffStatEffectPolicy : IEffectPolicy
    {
        public void Apply(Creature owner, Creature caster, EffectData effectData)
        {
            if (owner == null)
                return;

            EStatType statType = effectData.StatType;
            float value = effectData.StatAddValue;

            float prevValue = owner.GetTotalStat(statType);
            float finalValue = prevValue + value;
            owner.SetTotalStat(statType, finalValue);
        }

        public void Revert(Creature owner, EffectData effectData)
        {
            if (owner == null)
                return;

            EStatType statType = effectData.StatType;
            float value = effectData.StatAddValue;

            float prevValue = owner.GetTotalStat(statType);
            float finalValue = prevValue - value;
            owner.SetTotalStat(statType, finalValue);
        }
    }

    public class BuffStunPolicy : IEffectPolicy
    {
        public void Apply(Creature owner, Creature caster, EffectData effectData)
        {
            if (owner == null)
                return;

            owner.IsStunned = true;
        }

        public void Revert(Creature owner, EffectData effectData)
        {
            if (owner == null)
                return;

            owner.IsStunned = false;
        }
    }
}
