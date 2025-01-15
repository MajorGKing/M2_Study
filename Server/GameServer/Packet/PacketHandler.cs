using Google.Protobuf;
using Server;
using GameServer;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.Data;
using Google.Protobuf.Protocol;
using System.Numerics;
using GameServer.Game;

class PacketHandler
{
    ///////////////////////////////////// Client - Game Server /////////////////////////////////////

    public static void C_AuthReqHandler(PacketSession session, IMessage packet)
    {
        C_AuthReq reqPacket = (C_AuthReq)packet;
        ClientSession clientSession = (ClientSession)session;
        clientSession.HandleAuthReq(reqPacket);
    }

    public static void C_HeroListReqHandler(PacketSession session, IMessage packet)
    {
        C_HeroListReq reqPacket = (C_HeroListReq)packet;
        ClientSession clientSession = (ClientSession)session;
        clientSession.HandleHeroListReq();
    }

    public static void C_CreateHeroReqHandler(PacketSession session, IMessage packet)
    {
        C_CreateHeroReq reqPacket = (C_CreateHeroReq)packet;
        ClientSession clientSession = (ClientSession)session;
        clientSession.HandleCreateHeroReq(reqPacket);
    }

    public static void C_DeleteHeroReqHandler(PacketSession session, IMessage packet)
    {
        C_DeleteHeroReq repPacket = (C_DeleteHeroReq)packet;
        ClientSession clientSession = (ClientSession)session;
        clientSession.HandleDeleteHeroReq(repPacket);
    }
    public static void C_EnterGameHandler(PacketSession session, IMessage packet)
    {
        C_EnterGame enterGamePacket = (C_EnterGame)packet;
        ClientSession clientSession = (ClientSession)session;
        clientSession.HandleEnterGame(enterGamePacket);
    }

    public static void C_LeaveGameHandler(PacketSession session, IMessage packet)
    {
        C_LeaveGame enterGamePacket = (C_LeaveGame)packet;
        ClientSession clientSession = (ClientSession)session;
        clientSession.HandleLeaveGame();
    }

    public static void C_MoveHandler(PacketSession session, IMessage packet)
    {
        C_Move movePacket = (C_Move)packet;
        ClientSession clientSession = (ClientSession)session;

        Hero hero = clientSession.MyHero;
        if (hero == null)
            return;

        GameRoom room = hero.Room;
        if (room == null)
            return;

        room.Push(room.HandleMove, hero, movePacket);
    }

    public static void C_PongHandler(PacketSession session, IMessage packet)
    {
        ClientSession clientSession = (ClientSession)session;
        //clientSession.HandlePong();
    }

    public static void C_SkillHandler(PacketSession session, IMessage packet)
    {
        C_Skill skillPacket = packet as C_Skill;
        ClientSession clientSession = session as ClientSession;

        Hero hero = clientSession.MyHero;
        if (hero == null)
            return;

        GameRoom room = hero.Room;
        if (room == null)
            return;

        room.Push(room.UseSkill, hero, skillPacket.TemplateId, skillPacket.TargetId);
    }
}
