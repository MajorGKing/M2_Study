using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;
using System.Linq;
using static Define;

class PacketHandler
{
    ///////////////////////////////////// GameServer - Client /////////////////////////////////////
	public static void S_ConnectedHandler(PacketSession session, IMessage packet)
	{
		Debug.Log("S_Connected");
	}
	public static void S_EnterGameHandler(PacketSession session, IMessage packet)
	{
		Debug.Log("S_Connected");
		
		Managers.Scene.LoadScene(EScene.GameScene);
	}

	public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
	{
		Debug.Log("S_Connected");
	}
	
	public static void S_DespawnHandler(PacketSession session, IMessage packet)
	{
		Debug.Log("S_Connected");
	}
}
