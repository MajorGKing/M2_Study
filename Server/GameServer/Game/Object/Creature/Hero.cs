﻿using Google.Protobuf.Protocol;
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

        //public StatInfo TotalStat { get; private set; } = new StatInfo();

        // DB의 고유 번호
        public int HeroDbId { get; set; }

        // 남한테 보낼 때 사용하는 정보
        public HeroInfo HeroInfo { get; private set; } = new HeroInfo();
        // 스스로한테 보낼 때 사용하는 정보
        public MyHeroInfo MyHeroInfo { get; private set; } = new MyHeroInfo();

        // 플레이어 정보 관련
        public string Name
        {
            get { return HeroInfo.Name; }
            set { HeroInfo.Name = value; }
        }

        // 여기 들어올 때 ID 발급은 아직 안된 상태
        public Hero()
        {
            ObjectType = EGameObjectType.Hero;

            Vision = new VisionCubeComponent(this);
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
            MyHeroInfo.HeroInfo = HeroInfo;
            HeroInfo.CreatureInfo = CreatureInfo;

            InitializeHeroData();
            InitializeSkills();

            RefreshTotalStat();

            //TemplateId = ObjectManager.GetTemplateIdFromId(ObjectId);

            //if (DataManager.HeroDict.TryGetValue(TemplateId, out HeroData heroData))
            //{
            //    HeroData = heroData;

            //    BaseStat.MergeFrom(heroData.Stat.StatInfo);
            //    BaseStat.Hp = BaseStat.MaxHp;

            //    TotalStat.MergeFrom(BaseStat);

            //    SetupStatMappings();

            //    MyHeroInfo.TotalStatInfo = TotalStat;
            //    HeroInfo.CreatureInfo.StatInfo = TotalStat; // 플레이어는 최종 스탯으로 보내주도록

            //    //if (heroData.MainSkill != null)
            //    //    SkillBook.RegisterSkill(heroData.MainSkill.TemplateId);

            //    //if (heroData.SkillA != null)
            //    //    SkillBook.RegisterSkill(heroData.SkillA.TemplateId);

            //    //if (heroData.SkillB != null)
            //    //    SkillBook.RegisterSkill(heroData.SkillB.TemplateId);

            //    //if (heroData.SkillC != null)
            //    //    SkillBook.RegisterSkill(heroData.SkillC.TemplateId);

            //}

            //RefreshTotalStat();
        }

        public void RefreshTotalStat(bool notifyToClient = false)
        {
            if (notifyToClient)
            {
                S_ChangeStat changeStat = new S_ChangeStat();
                changeStat.TotalStatInfo = TotalStat;
                Session?.Send(changeStat);
            }
        }

        #region Init
        private void InitializeHeroData()
        {
            TemplateId = ObjectManager.GetTemplateIdFromId(ObjectId);

            if(DataManager.HeroDict.TryGetValue(TemplateId, out HeroData heroData))
            {
                HeroData = heroData;
                BaseStat.MergeFrom(heroData.Stat.StatInfo);
                BaseStat.Hp = BaseStat.MaxHp;
                TotalStat.MergeFrom(BaseStat);

                MyHeroInfo.TotalStatInfo = TotalStat;
                HeroInfo.CreatureInfo.StatInfo = TotalStat; // 플레이어는 최종 스탯으로 보내주도록
            }
        }

        // TODO
        private void InitializeSkills()
        {
            if (HeroData == null)
                return;

            SkillComp.RegisterSkill(HeroData.MainSkill.TemplateId);
            SkillComp.RegisterSkill(HeroData.SkillA.TemplateId);
            SkillComp.RegisterSkill(HeroData.SkillB.TemplateId);
            SkillComp.RegisterSkill(HeroData.SkillC.TemplateId);
        }
        #endregion
    }
}
