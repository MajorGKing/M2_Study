using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

namespace Scripts.Data
{
    #region Effect
   
    [CreateAssetMenu(fileName = "Assets/@Resources/Data/ScriptableObjectData/Effect/FILENAME", menuName = "Scriptable Objects/Effect", order = 0)]
    public class EffectData : ScriptableObject
    {
        public int TemplateId;
        public string Name;
        public string DescriptionTextID;
        public string PrefabName;
        public string SkeletonDataID;
        public string IconLabel;
        public string SoundLabel;
        public float Amount;
        public float PercentAdd;
        public float PercentMult;
        public float TickTime;
        public float TickCount;
        public int EffectType;
        public int calcStatType;
    }

    #endregion
}