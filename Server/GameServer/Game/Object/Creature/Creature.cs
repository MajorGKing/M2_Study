using GameServer.Game;
using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class Creature : BaseObject
    {
        public virtual CreatureData Data { get; set; }
        public SkillComponent SkillComp { get; protected set; }
        public EffectComponent EffectComp { get; protected set; }
        public CreatureInfo CreatureInfo { get; private set; } = new CreatureInfo();
        public StatInfo BaseStat { get; protected set; } = new StatInfo();
        public StatInfo TotalStat { get; protected set; } = new StatInfo();

        public static readonly Dictionary<EStatType, Func<StatInfo, float>> StatGetters = new Dictionary<EStatType, Func<StatInfo, float>>()
        {
            { EStatType.MaxHp, (s) => s.MaxHp },
            { EStatType.Hp, (s) => s.Hp },
            { EStatType.HpRegen, (s) => s.HpRegen },
            { EStatType.MaxMp, (s) => s.MaxMp },
            { EStatType.Mp, (s) => s.Mp },
            { EStatType.MpRegen, (s) => s.MpRegen },
            { EStatType.Attack, (s) => s.Attack },
            { EStatType.Defence, (s) => s.Defence },
            { EStatType.Dodge, (s) => s.Dodge },
            { EStatType.AttackSpeed, (s) => s.AttackSpeed },
            { EStatType.MoveSpeed, (s) => s.MoveSpeed },
            { EStatType.CriRate, (s) => s.CriRate },
            { EStatType.CriDamage, (s) => s.CriDamage },
            { EStatType.Str, (s) => s.Str },
            { EStatType.Dex, (s) => s.Dex },
            { EStatType.Int, (s) => s.Int },
            { EStatType.Con, (s) => s.Con },
            { EStatType.Wis, (s) => s.Wis }
        };

        public static readonly Dictionary<EStatType, Action<StatInfo, float>> StatSetters = new Dictionary<EStatType, Action<StatInfo, float>>()
        {
            { EStatType.MaxHp, (s, v) => s.MaxHp = v },
            { EStatType.Hp, (s, v) => s.Hp= v },
            { EStatType.HpRegen, (s, v) => s.HpRegen= v },
            { EStatType.MaxMp, (s, v) => s.MaxMp= v },
            { EStatType.Mp, (s, v) => s.Mp = v },
            { EStatType.MpRegen, (s, v) => s.MpRegen= v },
            { EStatType.Attack, (s, v) => s.Attack= v },
            { EStatType.Defence, (s, v) => s.Defence = v },
            { EStatType.Dodge, (s, v) => s.Dodge = v },
            { EStatType.AttackSpeed, (s, v) => s.AttackSpeed = v },
            { EStatType.MoveSpeed, (s, v) => s.MoveSpeed = v },
            { EStatType.CriRate, (s, v) => s.CriRate= v },
            { EStatType.CriDamage, (s, v) => s.CriDamage= v },
            { EStatType.Str, (s, v) => s.Str = (int)v },
            { EStatType.Dex, (s, v) => s.Dex = (int)v },
            { EStatType.Int, (s, v) => s.Int = (int)v },
            { EStatType.Con, (s, v) => s.Con = (int)v },
            { EStatType.Wis, (s, v) => s.Wis = (int)v }
        };

        public float GetBaseStat(EStatType statType) { return StatGetters[statType](BaseStat); }
        public float GetTotalStat(EStatType statType) { return StatGetters[statType](TotalStat); }
        public void SetBaseStat(EStatType statType, float value) { StatSetters[statType](BaseStat, value); }
        public void SetTotalStat(EStatType statType, float value)
        {
            StatSetters[statType](TotalStat, value);
            StatSetters[statType](CreatureInfo.TotalStatInfo, value);
        }

        public float Hp
        {
            get { return TotalStat.Hp; }
            set { TotalStat.Hp = Math.Clamp(value, 0, TotalStat.MaxHp); }
        }
        
        public float Mp
        {
            get { return TotalStat.Mp; }
            set { TotalStat.Mp = Math.Clamp(value, 0, TotalStat.MaxMp); }
        }

        public float MoveSpeed
        {
            get { return TotalStat.MoveSpeed; }
        }

        bool GetStateFlag(ECreatureStateFlag type)
        {
            return (CreatureInfo.StateFlag & (1 << (int)type)) != 0;
        }

        public void SetStateFlag(ECreatureStateFlag type, bool value)
        {
            if(value)
            {
                CreatureInfo.StateFlag |= (1 << (int)type);
            }
            else
            {
                CreatureInfo.StateFlag &= ~(1 << (int)type);
            }
        }

        public void ClearStateFlags()
        {
            for (int flag = 0; flag < (int)ECreatureStateFlag.MaxCount; flag++)
            {
                SetStateFlag((ECreatureStateFlag)flag, false);
            }
        }

        public bool IsStunned
        {
            get { return GetStateFlag(ECreatureStateFlag.Stun); }
            set { SetStateFlag(ECreatureStateFlag.Stun, value); }
        }

        public Creature()
        {
            CreatureInfo.ObjectInfo = ObjectInfo;

            SkillComp = new SkillComponent(this);
            EffectComp = new EffectComponent(this);
        }

        

        public override float OnDamaged(BaseObject attacker, float damage)
        {
            if (Room == null)
                return 0;

            if(State == EObjectState.Dead)
                return 0;

            // 데미지 감소
            damage = damage - (TotalStat.Defence * damage);
            TotalStat.Hp = Math.Max(TotalStat.Hp - damage, 0);

            S_ChangeHp changePacket = new S_ChangeHp();
            changePacket.ObjectId = ObjectId;
            changePacket.Hp = TotalStat.Hp;
            changePacket.Damage = damage;
            changePacket.DamageType = EDamageType.Hit;
            Room.Broadcast(CellPos, changePacket);

            if(TotalStat.Hp <= 0)
            {
                OnDead(attacker);
            }

            return damage;
        }

        public override void OnDead(BaseObject attacker)
        {
            base.OnDead(attacker);
        }

        public virtual bool IsEnemy(BaseObject target)
        {
            if (target == null)
                return false;
            if (target == this)
                return false;

            return Room == target.Room;
        }

        public virtual bool IsFriend(BaseObject target)
        {
            return IsEnemy(target) == false;
        }

        public virtual void Reset()
        {
            Console.WriteLine($"GetTotalStat(EStatType.MaxHp) {GetTotalStat(EStatType.MaxHp)}");
            Hp = Math.Max(0, GetTotalStat(EStatType.MaxHp));
            PosInfo.State = EObjectState.Idle;

            ClearStateFlags();
            EffectComp.Clear();
        }
    }
}
