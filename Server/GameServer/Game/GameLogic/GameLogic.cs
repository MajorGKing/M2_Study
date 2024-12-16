using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace GameServer
{
    // GameLogic
    // - GameRoom
    // -- Zone
    public class GameLogic : JobSerializer
    {
        public static GameLogic Instance { get; } = new GameLogic();

        Dictionary<int, GameRoom> _rooms = new Dictionary<int, GameRoom>();
        int _roomId = 1;

        public void Update()
        {
            Flush();

            foreach (GameRoom room in _rooms.Values)
            {
                room.Update();
            }
        }

        public GameRoom Add(int mapTemplateId)
        {
            GameRoom gameRoom = new GameRoom();
            gameRoom.Push(gameRoom.Init, mapTemplateId, 10);

            gameRoom.GameRoomId = _roomId;
            _rooms.Add(_roomId, gameRoom);
            _roomId++;

            return gameRoom;
        }

        public bool Remove(int roomId)
        {
            return _rooms.Remove(roomId);
        }

        public GameRoom Find(int roomId)
        {
            GameRoom room = null;
            if (_rooms.TryGetValue(roomId, out room))
                return room;

            return null;
        }

        public List<GameRoom> GetRooms()
        {
            return _rooms.Values.ToList();
        }
    }
}
