using Google.Protobuf;
using Server;
using Server.Game;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.Data;
using Google.Protobuf.Protocol;

class PacketHandler
{
    ///////////////////////////////////// Client - Game Server /////////////////////////////////////
    public static void C_TestHandler(PacketSession session, IMessage packet)
    {
        C_Test pkt = packet as C_Test;
        System.Console.WriteLine(pkt.Temp);
    }

}
