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
    public class CommonInteraction : INpcInteraction
    {
        private Npc _owner;
        private NpcData _npcData;

        public void SetInfo(Npc owner)
        {
            _owner = owner;

            if (DataManager.NpcDict.TryGetValue(_owner.TemplateId, out _npcData) == false)
                return;
        }

        public void HandleInteraction(Hero myHero)
        {
            if (_npcData == null)
                return;

            myHero.BroadcastEvent(EBroadcastEventType.InteractWithNpc, _npcData.TemplateId, 1);
        }

        public bool CanInteract(Hero myHero)
        {
            // 거리 판정.
            if (_npcData.Range < myHero.GetDistance(_owner))
                return false; 
            
            return true;
        }
    }
}
