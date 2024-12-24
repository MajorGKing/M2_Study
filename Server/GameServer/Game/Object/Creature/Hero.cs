using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Server;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GameServer
{
    public class Hero : Creature
    {
        public ClientSession Session { get; set; }
        public VisionCubeComponent Vision { get; protected set; }

        public StatInfo TotalStat { get; private set; } = new StatInfo();

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
            MyHeroInfo.HeroInfo = HeroInfo;
            MyHeroInfo.BaseStatInfo = BaseStat;
            HeroInfo.CreatureInfo = CreatureInfo;
            HeroInfo.CreatureInfo.StatInfo = TotalStat; // 플레이어는 최종 스탯으로 보내주도록

            Vision = new VisionCubeComponent(this);

            //TEMP
            if (DataManager.HeroDict.TryGetValue(1, out HeroData heroData))
            {
                BaseStat.Hp = heroData.Stat.StatInfo.MaxHp;
                BaseStat.MaxHp = heroData.Stat.StatInfo.MaxHp;
                BaseStat.Attack = heroData.Stat.StatInfo.Attack;
                BaseStat.Speed = heroData.Stat.StatInfo.Speed;
                BaseStat.Defence = heroData.Stat.StatInfo.Defence;
                BaseStat.CriRate = heroData.Stat.StatInfo.CriRate;
                BaseStat.CriDamage = heroData.Stat.StatInfo.CriDamage;
            }

            RefreshTotalStat();
        }

        public void RefreshTotalStat(bool notifyToClient = false)
        {
            TotalStat.MergeFrom(BaseStat);

            int extraAttack = 0;
            int extraDefence = 0;
            float extraSpeed = 0.0f;

            StatInfo curStat = new StatInfo();
            DataManager.HeroDict.TryGetValue(1, out HeroData data);
            curStat.MergeFrom(data.Stat.StatInfo);

            BaseStat.MaxHp = curStat.MaxHp;

            TotalStat.Attack = BaseStat.Attack + extraAttack;
            TotalStat.Defence = BaseStat.Defence + extraDefence;
            TotalStat.CriRate = BaseStat.CriRate;
            TotalStat.CriDamage = BaseStat.CriDamage;
            TotalStat.Defence = BaseStat.Defence + extraDefence;
            TotalStat.Speed = MathF.Max(1, extraSpeed) * BaseStat.Speed;
        }
    }
}
