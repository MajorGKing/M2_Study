using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Game
{
    public class PortalInteraction : INpcInteraction
    {
        private Npc _owner;
        private PortalData _portalData;
        public void SetInfo(Npc owner)
        {
            _owner = owner;

            if (DataManager.PortalDict.TryGetValue(_owner.TemplateId, out _portalData) == false)
                return;
        }
        public void HandleInteraction(Hero myHero)
        {
            GameRoom room = myHero.Room;
            if (room == null)
                return;

            GameRoom newRoom = GameLogic.Find(_portalData.DestPortal.OwnerRoomId);
            if (newRoom == null)
                return;

            if (room == newRoom)
            {
                // 텔레포트
            }
            else
            {
                Action job = () =>
                {
                    room.LeaveGame(myHero.ObjectId, ELeaveType.ChangeRoom);

                    // 새로운 방 입장.
                    Vector2Int spawnPos = new Vector2Int(_portalData.DestPortal.SpawnPosX, _portalData.DestPortal.SpawnPosY);
                    newRoom.Push(newRoom.EnterGame, myHero, spawnPos, false);
                };

                // 기존 방 퇴장.
                room.Push(job);
            }
        }

        public bool CanInteract(Hero myHero)
        {
            // 거리 판정.
            if (_portalData.Range < myHero.GetDistance(_owner))
                return false;

            return true;
        }


    }
}
