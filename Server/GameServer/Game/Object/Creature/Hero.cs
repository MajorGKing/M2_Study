using Google.Protobuf.Protocol;
using Server;
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
            // 참조 연결
            MyHeroInfo.HeroInfo = HeroInfo;
            HeroInfo.CreatureInfo = CreatureInfo;

            ObjectType = EGameObjectType.Hero;

            // TEMP
            //StatInfo statInfo = null;
            //DataManager.BaseStatDict.TryGetValue(1, out statInfo);

            Vision = new VisionCubeComponent(this);
        }
    }
}
