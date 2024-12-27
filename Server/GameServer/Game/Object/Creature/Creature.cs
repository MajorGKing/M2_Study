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

        public void Reset()
        {
            PosInfo.State = EObjectState.Idle;
        }
    }
}
