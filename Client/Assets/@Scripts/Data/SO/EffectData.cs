using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using Scripts.Data.SO;
using UnityEngine;

namespace Scripts.Data
{
    #region Effect
   
    [CreateAssetMenu(fileName = "Assets/@Resources/Data/ScriptableObjectData/Effect/FILENAME", menuName = "Scriptable Objects/Effect", order = 0)]
    public class EffectData : BaseData
    {
        public string SoundLabel;

        [Header("1. 효과 분류")]
        public EEffectType EffectType;

        // 즉발 vs 주기적 vs 영구적.
        [Header("2. 즉발 vs 주기적 vs 영구적")]
        public EDurationPolicy DurationPolicy;
        public float Duration;

        // EFFECT_TYPE_DAMAGE
        [Header("EFFECT_TYPE_DAMAGE")]
        public float DamageValue;

        // EFFECT_TYPE_HEAL
        [Header("EFFECT_TYPE_HEAL")]
        public float HealValue;

        // EFFECT_TYPE_BUFF_STAT
        [Header("EFFECT_TYPE_BUFF_STAT")]
        public EStatType StatType;
        public float StatAddValue;

        // EFFECT_TYPE_BUFF_LIFE_STEAL
        [Header("EFFECT_TYPE_BUFF_LIFE_STEAL")]
        public float LifeStealValue;

        // EFFECT_TYPE_BUFF_LIFE_STUN
        [Header("EFFECT_TYPE_BUFF_LIFE_STUN")]
        public float StunValue;
    }

    [Serializable]
    public class EffectDataLoader : ILoader<int, EffectData>
    {
        public List<EffectData> datas = new List<EffectData>();

        public Dictionary<int, EffectData> MakeDict()
        {
            Dictionary<int, EffectData> dict = new Dictionary<int, EffectData>();
            foreach (EffectData data in datas)
                dict.Add(data.TemplateId, data);

            return dict;
        }

        public bool Validate()
        {
            bool validate = true;
            return validate;
        }
    }
    #endregion
}