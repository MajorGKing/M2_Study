using Microsoft.EntityFrameworkCore;
using Server.Data;
using GameServer;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf.Protocol;

namespace Server
{
    public partial class ClientSession : PacketSession
    {
        public void HandleAuthReq(C_AuthReq reqPacket)
        {
            // TODO : Validation
            // reqPacket.Jwt

            S_AuthRes resPacket = new S_AuthRes();
            resPacket.Success = true;

            Send(resPacket);
        }

        public void HandleHeroListReq()
        {
            if(Heroes.Count == 0)
            {
                List<HeroDb> heroDbs = DBManager.LoadHeroDb(AccountDbId);
                foreach(HeroDb heroDb in heroDbs)
                {
                    Hero hero = MakeHeroFromHeroDb(heroDb);
                    Heroes.Add(hero);
                }
            }

            S_HeroListRes resPacket = new S_HeroListRes();
            foreach (Hero hero in Heroes)
            {
                Console.WriteLine($"Hero data{hero.Name}");
                resPacket.Heroes.Add(hero.MyHeroInfo);
            }

            Send(resPacket);
        }

        public void HandleEnterGame(C_EnterGame enterGamePacket)
        {
            // TODO : 인증 토큰 

            Console.WriteLine("HandleEnterGame");

            MyHero = ObjectManager.Instance.Spawn<Hero>(1);
            {
                MyHero.ObjectInfo.PosInfo.State = EObjectState.Idle;
                MyHero.ObjectInfo.PosInfo.MoveDir = EMoveDir.Down;
                MyHero.ObjectInfo.PosInfo.PosX = 0;
                MyHero.ObjectInfo.PosInfo.PosY = 0;
                MyHero.Session = this;
            }

            // TODO : DB에서 마지막 좌표 등 갖고 와서 처리.
            GameLogic.Instance.Push(() =>
            {
                GameRoom room = GameLogic.Instance.Find(1);

                room?.Push(() =>
                {
                    Hero hero = MyHero;
                    room.EnterGame(hero, respawn: false, pos: null);
                });
            });

        }
        Hero MakeHeroFromHeroDb(HeroDb heroDb)
        {
            Hero hero = ObjectManager.Instance.Spawn<Hero>(1);
            {
                hero.HeroDbId = heroDb.HeroDbId;
                hero.ObjectInfo.PosInfo.State = EObjectState.Idle;
                hero.ObjectInfo.PosInfo.PosX = heroDb.PosX;
                hero.ObjectInfo.PosInfo.PosY = heroDb.PosY;
                hero.HeroInfo.Level = heroDb.Level;
                hero.HeroInfo.Name = heroDb.Name;
                hero.HeroInfo.Gender = heroDb.Gender;
                hero.HeroInfo.ClassType = heroDb.ClassType;
                hero.Session = this;
            }

            return hero;
        }
    }
}
