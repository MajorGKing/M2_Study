using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using Scripts.Data.SO;
using UnityEngine;

namespace Scripts.Data
{
    #region Projectile
   
    [CreateAssetMenu(fileName = "Assets/@Resources/Data/ScriptableObjectData/Projectile/FILENAME", menuName = "Scriptable Objects/Projectile", order = 0)]
    public class ProjectileData : BaseData
    {
        public float Duration;
        public float ProjRange;
        public float ProjSpeed;
    }

    [Serializable]
    public class ProjectileDataLoader : ILoader<int, ProjectileData>
    {
        public List<ProjectileData> datas = new List<ProjectileData>();

        public Dictionary<int, ProjectileData> MakeDict()
        {
            Dictionary<int, ProjectileData> dict = new Dictionary<int, ProjectileData>();
            foreach (ProjectileData data in datas)
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