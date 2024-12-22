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

        public void HandleCreateHeroReq(C_CreateHeroReq reqPacket)
        {
            S_CreateHeroRes resPacket = new S_CreateHeroRes();

            // 1) 이름이 안 겹치는지 확인
            // 2) 생성 진행
            HeroDb heroDb = DBManager.CreateHeroDb(AccountDbId, reqPacket);
            if (heroDb != null)
            {
                resPacket.Result = ECreateHeroResult.Success;
                // 메모리에 캐싱
                Hero hero = MakeHeroFromHeroDb(heroDb);
                Heroes.Add(hero);
            }
            else
            {
                resPacket.Result = ECreateHeroResult.FailDuplicateName;
            }

            Send(resPacket);
        }

        public void HandleDeleteHeroReq(C_DeleteHeroReq reqPacket)
        {
            Console.WriteLine("HandleEnterGame");

            int index = reqPacket.HeroIndex;
            if (index < 0 || index >= Heroes.Count)
                return;

            Hero hero = Heroes[index];
            if (hero == null)
                return;

            bool sucess = DBManager.DeleteHeroDb(hero.HeroDbId);

            if(sucess)
            {
                Heroes.Remove(hero);
            }

            S_DeleteHeroRes resPacket = new S_DeleteHeroRes();
            resPacket.Success = sucess;
            reqPacket.HeroIndex = index;
            Send(resPacket);
        }

        public void HandleEnterGame(C_EnterGame enterGamePacket)
        {
            Console.WriteLine("HandleEnterGame");

            int index = enterGamePacket.HeroIndex;
            if (index < 0 || index >= Heroes.Count)
                return;

            MyHero = Heroes[index];

            Hero hero = MyHero;
            if(hero == null) 
                return;

            GameLogic.Instance.Push(() =>
            {
                GameRoom room = GameLogic.Instance.Find(1);

                room?.Push(() =>
                {
                    room.EnterGame(hero, respawn: false, pos: hero.CellPos);
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
