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
        public CreatureInfo CreatureInfo { get; private set; } = new CreatureInfo();
        public StatInfo BaseStat { get; protected set; } = new StatInfo();
        public StatInfo TotalStat { get; protected set; } = new StatInfo();
        public Dictionary<EStatType, Func<float>> BaseStats { get; set; }
        public Dictionary<EStatType, Func<float>> TotalStats { get; set; }


        public Creature()
        {
            CreatureInfo.ObjectInfo = ObjectInfo;
            //CreatureInfo.StatInfo = BaseStat;

        }

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
            { EStatType.MissChance, (s) => s.MissChance },
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
            { EStatType.MissChance, (s, v) => s.MissChance = v },
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

        public void SetupStatMappings()
        {
            BaseStats = new Dictionary<EStatType, Func<float>>
                {
                    { EStatType.MaxHp, () => BaseStat.MaxHp },
                    { EStatType.Hp, () => BaseStat.Hp },
                    { EStatType.HpRegen, () => BaseStat.HpRegen },
                    { EStatType.MaxMp, () => BaseStat.MaxMp },
                    { EStatType.Mp, () => BaseStat.Mp },
                    { EStatType.MpRegen, () => BaseStat.MpRegen },
                    { EStatType.Attack, () => BaseStat.Attack },
                    { EStatType.Defence, () => BaseStat.Defence },
                    { EStatType.MissChance, () => BaseStat.MissChance },
                    { EStatType.AttackSpeed, () => BaseStat.AttackSpeed },
                    { EStatType.MoveSpeed, () => BaseStat.MoveSpeed },
                    { EStatType.CriRate, () => BaseStat.CriRate },
                    { EStatType.CriDamage, () => BaseStat.CriDamage },
                    { EStatType.Str, () => BaseStat.Str },
                    { EStatType.Dex, () => BaseStat.Dex },
                    { EStatType.Int, () => BaseStat.Int },
                    { EStatType.Con, () => BaseStat.Con },
                    { EStatType.Wis, () => BaseStat.Wis }
                };

            TotalStats = new Dictionary<EStatType, Func<float>>
                {
                    { EStatType.MaxHp, () => TotalStat.MaxHp },
                    { EStatType.Hp, () => TotalStat.Hp },
                    { EStatType.HpRegen, () => TotalStat.HpRegen },
                    { EStatType.MaxMp, () => TotalStat.MaxMp },
                    { EStatType.Mp, () => TotalStat.Mp },
                    { EStatType.MpRegen, () => TotalStat.MpRegen },
                    { EStatType.Attack, () => TotalStat.Attack },
                    { EStatType.Defence, () => TotalStat.Defence },
                    { EStatType.MissChance, () => TotalStat.MissChance },
                    { EStatType.AttackSpeed, () => TotalStat.AttackSpeed },
                    { EStatType.MoveSpeed, () => TotalStat.MoveSpeed },
                    { EStatType.CriRate, () => TotalStat.CriRate },
                    { EStatType.CriDamage, () => TotalStat.CriDamage },
                    { EStatType.Str, () => TotalStat.Str },
                    { EStatType.Dex, () => TotalStat.Dex },
                    { EStatType.Int, () => TotalStat.Int },
                    { EStatType.Con, () => TotalStat.Con },
                    { EStatType.Wis, () => TotalStat.Wis }
                };
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

        public bool IsStunned
        {
            get { return GetStateFlag(ECreatureStateFlag.Stun); }
            set { SetStateFlag(ECreatureStateFlag.Stun, value); }
        }

        public virtual bool IsEnemy(BaseObject target)
        {
            if(target == null)
                return false;
            if (target == this)
                return false;

            return Room == target.Room;
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

            return damage;
        }

        public virtual bool IsFriend(BaseObject target)
        {
            return IsEnemy(target) == false;
        }

        public void Reset()
        {
            PosInfo.State = EObjectState.Idle;
        }
    }
}
