using GameServer.Game;
using Google.Protobuf.Protocol;
using Server.Data;
using ServerCore;
using System.Collections.ObjectModel;

namespace GameServer
{
    public class Creature : BaseObject
    {
        public virtual CreatureData Data { get; set; }
        public SkillComponent SkillComp { get; protected set; }
        public EffectComponent EffectComp { get; protected set; }
        public CreatureInfo CreatureInfo { get; private set; } = new CreatureInfo();
        public StatInfo BaseStat { get; protected set; } = new StatInfo();
        public StatInfo BonusStat { get; protected set; } = new StatInfo();
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
        public float GetBonusStat(EStatType statType) { return StatGetters[statType](BonusStat); }
        public float GetTotalStat(EStatType statType) { return StatGetters[statType](TotalStat); }
        public void SetBaseStat(EStatType statType, float value) { StatSetters[statType](BaseStat, value); }
        public void SetBonusStat(EStatType statType, float value) { StatSetters[statType](BonusStat, value); }
        protected void AddTotalStat(EStatType statType, float value)
        {
            float finalValue = GetTotalStat(statType) + value;
            SetTotalStat(statType, finalValue);
        }

        public void SetTotalStat(EStatType statType, float value)
        {
            StatSetters[statType](TotalStat, value);
            StatSetters[statType](CreatureInfo.TotalStatInfo, value);
        }

        public float Hp
        {
            get { return TotalStat.Hp; }
            set { SetTotalStat(EStatType.Hp, Math.Clamp(value, 0, TotalStat.MaxHp)); }
        }
        
        public float Mp
        {
            get { return TotalStat.Mp; }
            set { SetTotalStat(EStatType.Mp, Math.Clamp(value, 0, TotalStat.MaxMp)); }
        }

        public float Defence
        {
            get { return TotalStat.Defence; }
            set { SetTotalStat(EStatType.Defence, value); }
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
            float finalDamage = Math.Max(damage - Defence, 0);
            AddStat(EStatType.Hp, -finalDamage, EFontType.Hit);

            if (TotalStat.Hp <= 0)
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

        public void AddStat(EStatType statType, float diff, EFontType fontType, bool sendPacket = true)
        {
            if (diff == 0)
                return;

            if (State == EObjectState.Dead)
                return;

            AddTotalStat(statType, diff);

            if (sendPacket == false)
                return;

            S_ChangeOneStat changePacket = new S_ChangeOneStat();
            changePacket.ObjectId = ObjectId;
            changePacket.StatType = statType;
            changePacket.Value = GetTotalStat(statType);
            changePacket.Diff = diff;
            changePacket.FontType = fontType;

            // 다 보내고 클라에서 조건부로 처리
            Room?.Broadcast(CellPos, changePacket);

            //if (ObjectType == EGameObjectType.Hero)
            //{
            //	Hero hero = (Hero)this;
            //	hero.Session?.Send(changePacket);
            //}
            //else
            //{
            //	Room?.Broadcast(CellPos, changePacket);
            //}
        }

        public virtual void Reset()
        {
            Hp = Math.Max(0, GetTotalStat(EStatType.MaxHp));
            PosInfo.State = EObjectState.Idle;

            ClearStateFlags();
            EffectComp.Clear();
        }
    }
}
