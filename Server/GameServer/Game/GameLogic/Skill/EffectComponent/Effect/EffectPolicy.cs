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

            float damage = caster.GetTotalStat(EStatType.Attack) * effectData.DamageValue;

            //Console.WriteLine($"{owner.ObjectId} Try {caster.ObjectId} HP : {owner.Hp} \tDamaged : {damage}");

            if(effectData.DamageValue > 0)
                owner.OnDamaged(caster, damage);
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
