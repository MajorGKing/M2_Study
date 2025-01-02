using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;

public enum MsgId
{
	S_Connected = 1,
	C_AuthReq = 2,
	S_AuthRes = 3,
	C_HeroListReq = 4,
	S_HeroListRes = 5,
	C_CreateHeroReq = 6,
	S_CreateHeroRes = 7,
	C_DeleteHeroReq = 8,
	S_DeleteHeroRes = 9,
	C_EnterGame = 10,
	S_EnterGame = 11,
	C_LeaveGame = 12,
	S_LeaveGame = 13,
	S_Spawn = 14,
	S_Despawn = 15,
	C_Move = 16,
	S_Move = 17,
	S_Ping = 18,
	C_Pong = 19,
	C_Skill = 20,
	S_Skill = 21,
	S_ChangeHp = 22,
	S_ChangeEffects = 23,
	S_Die = 24,
	S_ChangeStat = 25,
}

class PacketManager
{
	#region Singleton
	static PacketManager _instance = new PacketManager();
	public static PacketManager Instance { get { return _instance; } }
	#endregion

	PacketManager()
	{
		Register();
	}

	Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>>();
	Dictionary<ushort, Action<PacketSession, IMessage>> _handler = new Dictionary<ushort, Action<PacketSession, IMessage>>();
		
	public Action<PacketSession, IMessage, ushort> CustomHandler { get; set; }

	public void Register()
	{		
		_onRecv.Add((ushort)MsgId.S_Connected, MakePacket<S_Connected>);
		_handler.Add((ushort)MsgId.S_Connected, PacketHandler.S_ConnectedHandler);		
		_onRecv.Add((ushort)MsgId.S_AuthRes, MakePacket<S_AuthRes>);
		_handler.Add((ushort)MsgId.S_AuthRes, PacketHandler.S_AuthResHandler);		
		_onRecv.Add((ushort)MsgId.S_HeroListRes, MakePacket<S_HeroListRes>);
		_handler.Add((ushort)MsgId.S_HeroListRes, PacketHandler.S_HeroListResHandler);		
		_onRecv.Add((ushort)MsgId.S_CreateHeroRes, MakePacket<S_CreateHeroRes>);
		_handler.Add((ushort)MsgId.S_CreateHeroRes, PacketHandler.S_CreateHeroResHandler);		
		_onRecv.Add((ushort)MsgId.S_DeleteHeroRes, MakePacket<S_DeleteHeroRes>);
		_handler.Add((ushort)MsgId.S_DeleteHeroRes, PacketHandler.S_DeleteHeroResHandler);		
		_onRecv.Add((ushort)MsgId.S_EnterGame, MakePacket<S_EnterGame>);
		_handler.Add((ushort)MsgId.S_EnterGame, PacketHandler.S_EnterGameHandler);		
		_onRecv.Add((ushort)MsgId.S_LeaveGame, MakePacket<S_LeaveGame>);
		_handler.Add((ushort)MsgId.S_LeaveGame, PacketHandler.S_LeaveGameHandler);		
		_onRecv.Add((ushort)MsgId.S_Spawn, MakePacket<S_Spawn>);
		_handler.Add((ushort)MsgId.S_Spawn, PacketHandler.S_SpawnHandler);		
		_onRecv.Add((ushort)MsgId.S_Despawn, MakePacket<S_Despawn>);
		_handler.Add((ushort)MsgId.S_Despawn, PacketHandler.S_DespawnHandler);		
		_onRecv.Add((ushort)MsgId.S_Move, MakePacket<S_Move>);
		_handler.Add((ushort)MsgId.S_Move, PacketHandler.S_MoveHandler);		
		_onRecv.Add((ushort)MsgId.S_Ping, MakePacket<S_Ping>);
		_handler.Add((ushort)MsgId.S_Ping, PacketHandler.S_PingHandler);		
		_onRecv.Add((ushort)MsgId.S_Skill, MakePacket<S_Skill>);
		_handler.Add((ushort)MsgId.S_Skill, PacketHandler.S_SkillHandler);		
		_onRecv.Add((ushort)MsgId.S_ChangeHp, MakePacket<S_ChangeHp>);
		_handler.Add((ushort)MsgId.S_ChangeHp, PacketHandler.S_ChangeHpHandler);		
		_onRecv.Add((ushort)MsgId.S_ChangeEffects, MakePacket<S_ChangeEffects>);
		_handler.Add((ushort)MsgId.S_ChangeEffects, PacketHandler.S_ChangeEffectsHandler);		
		_onRecv.Add((ushort)MsgId.S_Die, MakePacket<S_Die>);
		_handler.Add((ushort)MsgId.S_Die, PacketHandler.S_DieHandler);		
		_onRecv.Add((ushort)MsgId.S_ChangeStat, MakePacket<S_ChangeStat>);
		_handler.Add((ushort)MsgId.S_ChangeStat, PacketHandler.S_ChangeStatHandler);
	}

	public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
	{
		ushort count = 0;

		ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
		count += 2;
		ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
		count += 2;

		Action<PacketSession, ArraySegment<byte>, ushort> action = null;
		if (_onRecv.TryGetValue(id, out action))
			action.Invoke(session, buffer, id);
	}

	void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer, ushort id) where T : IMessage, new()
	{
		T pkt = new T();
		pkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);

		if (CustomHandler != null)
		{
			CustomHandler.Invoke(session, pkt, id);
		}
		else
		{
			Action<PacketSession, IMessage> action = null;
			if (_handler.TryGetValue(id, out action))
				action.Invoke(session, pkt);
		}
	}

	public Action<PacketSession, IMessage> GetPacketHandler(ushort id)
	{
		Action<PacketSession, IMessage> action = null;
		if (_handler.TryGetValue(id, out action))
			return action;
		return null;
	}
}