using GameServer.Game;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game;
using System.Net.Http.Headers;
using System.Numerics;

namespace GameServer
{
    public partial class GameRoom : JobSerializer
    {
        public void HandleInteractionNpc(Hero myHero, int npcObjectId)
        {
            if (myHero.IsValid() == false)
                return;

            if (_npcs.TryGetValue(npcObjectId, out Npc npc) == false)
                return;

            if (npc.Interaction.CanInteract(myHero) == false)
                return;

            npc.Interaction.HandleInteraction(myHero);
        }
    }
}
