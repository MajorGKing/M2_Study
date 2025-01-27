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

            // ILAHK TODO
            foreach (StatValuePair pair in effectData.StatValues)
            {
                float value = pair.AddValue;
                float prevValue = owner.GetTotalStat(pair.StatType);
                float finalValue = prevValue + value;
                owner.SetTotalStat(pair.StatType, finalValue);
            }
        }

        public void Revert(Creature owner, EffectData effectData)
        {
            if (owner == null)
                return;

            foreach (StatValuePair pair in effectData.StatValues)
            {
                float value = pair.AddValue;
                float prevValue = owner.GetTotalStat(pair.StatType);
                float finalValue = prevValue - value;
                owner.SetTotalStat(pair.StatType, finalValue);
            }
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

        public class HealEffectPolicy : IEffectPolicy
        {
            public void Apply(Creature owner, Creature caster, EffectData effectData)
            {
                foreach(StatValuePair pair in effectData.StatValues)
                {
                    owner.Heal(pair.StatType, (int)pair.AddValue);
                }
            }

            public void Revert(Creature owner, EffectData effectData)
            {
            }
        }
    }
}
