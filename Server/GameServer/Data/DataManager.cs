using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GameServer
{
    public interface ILoader<Key, Value>
    {
        Dictionary<Key, Value> MakeDict();
    }

    public class DataManager
    {
        public static Dictionary<int, HeroData> HeroDict { get; private set; } = new Dictionary<int, HeroData>();
        public static Dictionary<int, MonsterData> MonsterDict { get; private set; } = new Dictionary<int, MonsterData>();
        public static Dictionary<int, SpawningPoolData> SpawningPoolDict { get; private set; } = new Dictionary<int, SpawningPoolData>();

        public static void LoadData()
        {
            HeroDict = LoadJson<HeroDataLoader, int, HeroData>("HeroData").MakeDict();
            MonsterDict = LoadJson<MonsterDataLoader, int, MonsterData>("MonsterData").MakeDict();
            SpawningPoolDict = LoadJson<SpawningPoolDataLoader, int, SpawningPoolData>("SpawningPoolData").MakeDict();
        }

        static Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
        {
            string text = File.ReadAllText($"{ConfigManager.Config.dataPath}/JsonData/{path}.json");
            Console.WriteLine(path);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Loader>(text);
        }

        static Loader LoadJson<Loader>(string path)
        {
            string text = File.ReadAllText($"{ConfigManager.Config.dataPath}/JsonData/{path}.json");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Loader>(text);
        }
    }
}
