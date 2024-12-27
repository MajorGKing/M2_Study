using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Data.SO
{
    public class CreatureData : ScriptableObject
    {
        public int TemplateId;
        public string NameTextId;
        public string IconImage;
        public string PrefabName;
        public StatInfoWrapper Stat;

        public virtual bool Validate()
        {
            return true;
        }
    }

    //protobuf ¿¬µ¿
    [Serializable]
    public class StatInfoWrapper
    {
        public float MaxHp;
        public float Hp;
        public float HpRegen;
        public float MaxMp;
        public float Mp;
        public float MpRegen;
        public float Attack;
        public float Defence;
        public float MissChance;
        public float AttackSpeed;
        public float MoveSpeed;
        public float CriRate;
        public float CriDamage;
        public int Str;
        public int Dex;
        public int Int;
        public int Con;
        public int Wis;

        public StatInfo StatInfo
        {
            get
            {
                return new StatInfo
                {
                    MaxHp = this.MaxMp,
                    Hp = this.Hp,
                    HpRegen = this.HpRegen,
                    MaxMp = this.MaxMp,
                    Mp = this.Mp,
                    MpRegen = this.MpRegen,
                    Attack = this.Attack,
                    Defence = this.Defence,
                    MissChance = this.MissChance,
                    AttackSpeed = this.AttackSpeed,
                    MoveSpeed = this.MoveSpeed,
                    CriRate = this.CriRate,
                    CriDamage = this.CriDamage,
                    Str = this.Str,
                    Dex = this.Dex,
                    Int = this.Int,
                    Con = this.Con,
                    Wis = this.Wis,
                };
            }
        }
    }
}
