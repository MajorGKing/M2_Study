using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GameServer
{
    public interface IValidate
    {
        bool Validate();
    }

    public interface ILoader<Key, Value> : IValidate
    {
        Dictionary<Key, Value> MakeDict();
    }

    public class DataManager
    {
        static HashSet<IValidate> _loaders = new HashSet<IValidate>();

        public static Dictionary<int, BaseStatData> BaseStatDic { get; private set; } = new Dictionary<int, BaseStatData>();
        public static Dictionary<int, HeroData> HeroDict { get; private set; } = new Dictionary<int, HeroData>();
        public static Dictionary<int, MonsterData> MonsterDict { get; private set; } = new Dictionary<int, MonsterData>();
        public static Dictionary<int, SkillData> SkillDict { get; private set; } = new Dictionary<int, SkillData>();
        public static Dictionary<int, EffectData> EffectDict { get; private set; } = new Dictionary<int, EffectData>();
        public static Dictionary<int, SpawningPoolData> SpawningPoolDict { get; private set; } = new Dictionary<int, SpawningPoolData>();
        public static Dictionary<int, ProjectileData> ProjectileDict { get; private set; } = new Dictionary<int, ProjectileData>();
        public static Dictionary<int, RewardData> RewardDict { get; private set; } = new Dictionary<int, RewardData>();
        public static Dictionary<int, DropTableData> DropTableDict { get; private set; } = new Dictionary<int, DropTableData>();
        public static Dictionary<int, RoomData> RoomDict { get; private set; } = new Dictionary<int, RoomData>();

        public static Dictionary<int, NpcData> NpcDict { get; private set; } = new Dictionary<int, NpcData>();
        public static Dictionary<int, PortalData> PortalDict { get; private set; } = new Dictionary<int, PortalData>();

        public static Dictionary<int, ItemData> ItemDict { get; private set; } = new Dictionary<int, ItemData>();
        public static Dictionary<int, EquipmentData> EquipmentDict { get; private set; } = new Dictionary<int, EquipmentData>();
        public static Dictionary<int, ConsumableData> ConsumableDict { get; private set; } = new Dictionary<int, ConsumableData>();


        public static void LoadData()
        {
            BaseStatDic = LoadJson<BaseStatDataLoader, int, BaseStatData>("BaseStatData").MakeDict();
            HeroDict = LoadJson<HeroDataLoader, int, HeroData>("HeroData").MakeDict();
            MonsterDict = LoadJson<MonsterDataLoader, int, MonsterData>("MonsterData").MakeDict();
            EffectDict = LoadJson<EffectDataLoader, int, EffectData>("EffectData").MakeDict();
            SkillDict = LoadJson<SkillDataLoader, int, SkillData>("SkillData").MakeDict();
            SpawningPoolDict = LoadJson<SpawningPoolDataLoader, int, SpawningPoolData>("SpawningPoolData").MakeDict();
            ProjectileDict = LoadJson<ProjectileDataLoader, int, ProjectileData>("ProjectileData").MakeDict();
            RewardDict = LoadJson<RewardDataLoader, int, RewardData>("RewardData").MakeDict();
            DropTableDict = LoadJson<DropTableDataLoader, int, DropTableData>("DropTableData").MakeDict();
            RoomDict = LoadJson<RoomDataLoader, int, RoomData>("RoomData").MakeDict();

            #region ItemData
            EquipmentDict = LoadJson<EquipmentDataLoader, int, EquipmentData>("EquipmentData").MakeDict();
            ConsumableDict = LoadJson<ConsumableDataLoader, int, ConsumableData>("ConsumableData").MakeDict();

            ItemDict.Clear();

            foreach (var item in EquipmentDict)
                ItemDict.Add(item.Key, item.Value);

            foreach (var item in ConsumableDict)
                ItemDict.Add(item.Key, item.Value);
            #endregion

            #region NpcData
            PortalDict = LoadJson<PortalDataLoader, int, PortalData>("PortalData").MakeDict();

            NpcDict.Clear();
            foreach (var portal in PortalDict)
            {
                NpcDict.Add(portal.Key, portal.Value);
            }
            #endregion

            Validate();
        }

        static Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
        {
            Console.WriteLine(path);
            string text = File.ReadAllText($"{ConfigManager.Config.dataPath}/JsonData/{path}.json");
            Loader loader = Newtonsoft.Json.JsonConvert.DeserializeObject<Loader>(text);
            _loaders.Add(loader);

            return loader;
        }

        static Loader LoadJson<Loader>(string path)
        {
            string text = File.ReadAllText($"{ConfigManager.Config.dataPath}/JsonData/{path}.json");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Loader>(text);
        }

        static bool Validate()
        {
            bool success = true;

            foreach (var loader in _loaders)
            {
                if (loader.Validate() == false)
                    success = false;
            }

            _loaders.Clear();

            return success;
        }
    }
}
