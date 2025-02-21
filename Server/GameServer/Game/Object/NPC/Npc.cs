using Google.Protobuf.Protocol;
using Microsoft.VisualBasic;
using Server.Data;
using Server.Game;

namespace GameServer.Game
{
    public interface INpcInteraction
    {
        public void SetInfo(Npc owner);
        public void HandleInteraction(Hero myHero);
        public bool CanInteract(Hero myHero);
    }

    public class Npc : BaseObject
    {
        public NpcData NpcData;
        public INpcInteraction Interaction { get;private set; }

        public Npc()
        {
            ObjectType = EGameObjectType.Npc;
        }

        public virtual void Init(int templateId)
        {
            if (DataManager.NpcDict.TryGetValue(templateId, out NpcData) == false)
                return;

            TemplateId = templateId;
            ExtraCells = NpcData.ExtraCells;
            SetInteraction();
        }

        private void SetInteraction()
        {
            switch(NpcData.NpcType)
            {
                case ENpcType.Portal:
                    Interaction = new PortalInteraction();
                    break;
                case ENpcType.Shop:
                    break;
                default:
                    Interaction = new CommonInteraction();
                    break;
            }

            Interaction?.SetInfo(this);
        }
    }
}
