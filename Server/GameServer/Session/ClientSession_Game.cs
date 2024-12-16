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
    }
}
