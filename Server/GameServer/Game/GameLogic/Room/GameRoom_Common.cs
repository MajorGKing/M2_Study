using GameServer.Game;
using Google.Protobuf.Protocol;

namespace GameServer
{
    public partial class GameRoom : JobSerializer
    {
        public void HandleFillCollection(Hero myHero, C_FillCollection pkt)
        {
            if (myHero.IsValid() == false) 
                return;

            myHero.CollectionComp.TryFillCollection(pkt.TemplateId, pkt.ItemDbId, pkt.Index);
        }

    }
}
