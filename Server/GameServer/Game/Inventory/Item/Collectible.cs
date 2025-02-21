using Google.Protobuf.Protocol;
using Server.Data;

namespace Server.Game
{
    public class Collectible : Item
    {
        public CollectibleData CollectibleData { get; private set; }

        public Collectible(int templateId) : base(templateId)
        {
            Init();
        }

        void Init()
        {
            if (TemplateData == null)
                return;

            if (TemplateData.Type != EItemType.Collectible)
                return;

            CollectibleData = (CollectibleData)TemplateData;
        }
    }
}
