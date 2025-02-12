using Google.Protobuf.Protocol;
using Server;
using Server.Data;
using Server.Game;

namespace GameServer
{
    public class Hero : Creature
    {
        public HeroData HeroData { get; set; }
        public override CreatureData Data
        {
            get { return HeroData; }
        }

        public ClientSession Session { get; set; }
        public VisionCubeComponent Vision { get; protected set; }

        public InventoryComponent Inven { get; private set; }

        //public StatInfo TotalStat { get; private set; } = new StatInfo();

        // DB의 고유 번호
        public int HeroDbId { get; set; }

        // 남한테 보낼 때 사용하는 정보
        public HeroInfo HeroInfo { get; set; } = new HeroInfo();
        // 스스로한테 보낼 때 사용하는 정보
        public MyHeroInfo MyHeroInfo { get; set; } = new MyHeroInfo();

        #region HeroInfo Values

        public int Level
        {
            get { return MyHeroInfo.HeroInfo.Level; }
            private set { MyHeroInfo.HeroInfo.Level = value; }
        }

        public int Gold
        {
            get { return MyHeroInfo.CurrencyInfo.Gold; }
            private set { MyHeroInfo.CurrencyInfo.Gold = value; }
        }

        public int Dia
        {
            get { return MyHeroInfo.CurrencyInfo.Dia; }
            private set { MyHeroInfo.CurrencyInfo.Dia = value; }
        }

        public int Exp
        {
            get { return MyHeroInfo.Exp; }
            private set { MyHeroInfo.Exp = value; }
        }

        // 플레이어 정보 관련
        public string Name
        {
            get { return HeroInfo.Name; }
            set { HeroInfo.Name = value; }
        }

        public int MapId
        {
            get { return MyHeroInfo.MapId; }
            set { MyHeroInfo.MapId = value; }
        }
        #endregion

        // 여기 들어올 때 ID 발급은 아직 안된 상태
        public Hero()
        {
            ObjectType = EGameObjectType.Hero;

            Vision = new VisionCubeComponent(this);
            Inven = new InventoryComponent(this);
        }

        public void Init(HeroDb heroDb)
        {
            HeroDbId = heroDb.HeroDbId;
            //Pos
            ObjectInfo.PosInfo.State = EObjectState.Idle;
            ObjectInfo.PosInfo.PosX = heroDb.PosX;
            ObjectInfo.PosInfo.PosY = heroDb.PosY;

            //HeroInfo
            HeroInfo.Level = heroDb.Level;
            HeroInfo.Name = heroDb.Name;
            HeroInfo.Gender = heroDb.Gender;
            HeroInfo.ClassType = heroDb.ClassType;

            MyHeroInfo.MapId = heroDb.MapId;
            MyHeroInfo.HeroInfo = HeroInfo;
            MyHeroInfo.Exp = heroDb.Exp;
            HeroInfo.CreatureInfo = CreatureInfo;
            MyHeroInfo.HeroInfo.CreatureInfo.TotalStatInfo = TotalStat;
            MyHeroInfo.CurrencyInfo = new CurrencyInfo()
            {
                Gold = heroDb.Gold,
                Dia = heroDb.Dia,
            };

            //MyHeroInfo.
            InitializeHeroData(heroDb);
            InitializeSkills();
            InitializeItems(heroDb);
        }

        public void SendRefreshStat()
        {
            S_RefreshStat changeStat = new S_RefreshStat();
            changeStat.TotalStatInfo = TotalStat;
            Session?.Send(changeStat);
        }

        public void RefreshStat()
        {
            //Temp 물약버프같은거는 clear X
            EffectComp.Clear();
            //BaseStat, TotalStat
            InitStat(Level);

            //장비아이템 refresh
            Inven.ApplyEquipmentEffects();

            SendRefreshStat();
        }


        #region Init
        private void InitStat(int level, bool MaxHp = true)
        {
            //레벨별 baseStat 계산
            if (DataManager.BaseStatDic.TryGetValue(level, out BaseStatData baseStatData))
            {
                BaseStat.Attack = baseStatData.Attack;
                BaseStat.MaxHp = baseStatData.MaxHp;
                BaseStat.MaxMp = baseStatData.MaxMp;
                BaseStat.Hp = baseStatData.MaxHp;
                BaseStat.Mp = baseStatData.MaxMp;
                BaseStat.HpRegen = baseStatData.HpRegen;
                BaseStat.MpRegen = baseStatData.MpRegen;
                BaseStat.Defence = baseStatData.Def;
                BaseStat.Dodge = baseStatData.Dodge;
                BaseStat.AttackSpeed = baseStatData.AtkSpeed;
                BaseStat.MoveSpeed = baseStatData.MoveSpeed;
                BaseStat.CriRate = baseStatData.CriRate;
                BaseStat.CriDamage = baseStatData.CriDamage;
                BaseStat.Str = baseStatData.Str;
                BaseStat.Dex = baseStatData.Dex;
                BaseStat.Int = baseStatData.Int;
                BaseStat.Con = baseStatData.Con;
                BaseStat.Wis = baseStatData.Wis;
            }

            TotalStat.MergeFrom(BaseStat);
            MyHeroInfo.HeroInfo.CreatureInfo.TotalStatInfo.MergeFrom(TotalStat);

            if (MaxHp)
            {
                SetTotalStat(EStatType.Hp, TotalStat.MaxHp);
                SetTotalStat(EStatType.Mp, TotalStat.MaxMp);
            }
        }
        private void InitializeHeroData(HeroDb heroDb)
        {
            TemplateId = ObjectManager.GetTemplateIdFromId(ObjectId);

            if(DataManager.HeroDict.TryGetValue(TemplateId, out HeroData heroData))
            {
                HeroData = heroData;
                BaseStat.MergeFrom(heroData.Stat);

                InitStat(heroDb.Level, false);
            }

            ////DB 저장된 HP/MP가 없다면 풀피
            Hp = heroDb.Hp == -1 ? BaseStat.MaxHp : heroDb.Hp;
            Mp = heroDb.Mp == -1 ? BaseStat.MaxMp : heroDb.Mp;
        }

        private void InitializeSkills()
        {
            if (HeroData == null)
                return;

            foreach (var skillData in HeroData.SkillMap.Values)
            {
                SkillComp.RegisterSkill(skillData.TemplateId);
            }
        }

        private void InitializeItems(HeroDb heroDb)
        {
            Inven.Init(heroDb.Items.ToList());

            //장착한 아이템 이펙트 적용
            Inven.ApplyEquipmentEffects();
        }

        public override void Reset()
        {
            base.Reset();
            //장착한 아이템 이펙트 적용
            Inven.ApplyEquipmentEffects();
        }
        #endregion

        #region Battle
        private IJob _onDeadJob;
        public void ReserveRebirth()
        {
            GameRoom room = Room;
            _onDeadJob = Room?.PushAfter(3000, () =>
            {
                room.EnterGame(this, cellPos: CellPos);
            });
        }

        public void CancelJobs()
        {
            if (_onDeadJob != null)
                _onDeadJob.Cancel = true;
        }
        #endregion

        #region BonusStat
        public void AddItemBonusStat(EquipmentData data)
        {
            // MaxHp
            float prev = GetTotalStat(EStatType.MaxHp);
            SetTotalStat(EStatType.MaxHp, prev + data.MaxHpBonus);
            // Attack
            Attack += data.AttackBonus;
            // Defence
            Defence += data.DefenceBonus;
        }

        public void RemoveItemBonusStat(EquipmentData data)
        {
            // MaxHp
            float prev = GetTotalStat(EStatType.MaxHp);
            SetTotalStat(EStatType.MaxHp, prev - data.MaxHpBonus);
            // Attack
            Attack -= data.AttackBonus;
            // Defence
            Defence -= data.DefenceBonus;
        }
        #endregion

        #region Level System
        public void AddExp(int amount)
        {
            if (IsMaxLevel())
                return;

            Exp += amount;
            while (!IsMaxLevel() && Exp >= GetExpToNextLevel(Level))
            {
                Exp -= GetExpToNextLevel(Level);
                Level++;
                RefreshStat();
            }
        }

        public bool CanLevelUp()
        {
            return (GetExpToNextLevel(Level) - Exp <= 0);
        }

        public float GetExpNormalized()
        {
            if (IsMaxLevel())
            {
                return 1f;
            }

            return (float)Exp / GetExpToNextLevel(Level);
        }

        public int GetExpToNextLevel(int level)
        {
            if (DataManager.BaseStatDic.TryGetValue(level, out BaseStatData data))
            {
                return data.Exp;
            }
            else
            {
                return 100;
            }
        }

        public bool IsMaxLevel()
        {
            return IsMaxLevel(Level);
        }

        public bool IsMaxLevel(int level)
        {
            return level == DataManager.BaseStatDic.Count;
        }
        #endregion

        public void RewardExpAndGold(DropTableData dropTable)
        {
            AddExp(dropTable.RewardExp);
            Gold += dropTable.RewardGold;

            S_RewardValue packet = new S_RewardValue()
            {
                Exp = dropTable.RewardExp,
                Gold = dropTable.RewardGold,
            };
            Session?.Send(packet);
        }
    }
}
