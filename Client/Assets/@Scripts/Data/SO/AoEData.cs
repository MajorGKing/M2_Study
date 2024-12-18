using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

namespace Scripts.Data
{
    #region AOE
   
    [CreateAssetMenu(fileName = "Assets/@Resources/Data/ScriptableObjectData/AOE/FILENAME", menuName = "Scriptable Objects/AOE", order = 0)]
    public class AOEData : ScriptableObject
    {
        public int TemplateId;
        public string Name;
        public int AoEType;
        public string SkeletonDataID;
        public string PrefabName;
        public string SoundLabel;
        public float Duration;
        public List<EffectData> AllyEffects;
        public List<EffectData> EnemyEffects;
        public string AnimName;
        public int FindRangeType;
    }

    #endregion
}