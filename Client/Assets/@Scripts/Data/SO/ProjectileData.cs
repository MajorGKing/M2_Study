using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

namespace Scripts.Data
{
    #region Projectile
   
    [CreateAssetMenu(fileName = "Assets/@Resources/Data/ScriptableObjectData/Projectile/FILENAME", menuName = "Scriptable Objects/Projectile", order = 0)]
    public class ProjectileData : ScriptableObject
    {
        public int TemplateId;
        public string Name;
        public string PrefabName;
        public int ProjectileMotionType;
        public string SpriteName;
        public string SpineName;
        public float Duration;
        public float HitSound;
        public float ProjRange;
        public float ProjSpeed;
        public int PenetrateCount;
    }
    
    #endregion
}