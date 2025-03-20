using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game;

namespace GameServer.Game
{
    public class Collection
    {
        public CollectionInfo Info { get; } = new CollectionInfo();
        public CollectionData CollectionData { get; private set; }
        public Hero Owner { get; set; }

        public bool IsCompleted
        {
            get
            {
                int totalItems = CollectionData?.ItemIds.Count ?? 0;
                int completeMask = (1 << totalItems) - 1;
                return (ProgressFlag & completeMask) == completeMask;
            }
        }

        public Collection(int templateId, Hero owner)
        {
            if (DataManager.CollectionDict.TryGetValue(templateId, out CollectionData collectionData) == false)
                return;

            // 기본 정보 설정.
            TemplateId = templateId;
            Owner = owner;
            CollectionData = collectionData;
            Info.ProgressFlag = 0;
        }

        public Collection(CollectionDb collectionDb, Hero owner) : this(collectionDb.TemplateId, owner)
        {
            Info.ProgressFlag = collectionDb.ProgressFlag;
        }

        public int GetCollectionItemTemplateId(int idx)
        {
            if (idx < 0 || idx >= CollectionData.ItemIds.Count)
                return 0;

            return CollectionData.ItemIds[idx];
        }

        public bool CanCollect(Item item, int index)
        {
            if (IsCompleted)
                return false;

            if (item == null)
                return false;

            int itemTemplateId = GetCollectionItemTemplateId(index);
            if (itemTemplateId == 0)
                return false;

            if (item.TemplateId != itemTemplateId)
                return false;

            if (IsItemCollectedAtIndex(index))
                return false;

            return true;
        }

        public bool IsItemCollectedAtIndex(int index)
        {
            return (ProgressFlag & (1 << index)) != 0;
        }

        public void CollectItem(Item item, int index, bool sendToClient = false)
        {
            if (CanCollect(item, index) == false)
                return;

            // 메모리 갱신.
            ProgressFlag |= (1 << index);

            // 클라한테 전송.
            if (sendToClient)
                SendToClient(Info);

            // 완료 여부 확인.
            if (IsCompleted)
                Owner.BroadcastHeroInternalEvent(EHeroInternalEventType.CompleteCollection, TemplateId);
        }

        #region Helpers
        public int TemplateId
        {
            get { return Info.TemplateId; }
            set { Info.TemplateId = value; }
        }

        public int ProgressFlag
        {
            get { return Info.ProgressFlag; }
            set { Info.ProgressFlag = value; }
        }

        public static Collection MakeCollection(CollectionDb collectionDb, Hero owner)
        {
            if (DataManager.CollectionDict.ContainsKey(collectionDb.TemplateId) == false)
                return null;

            Collection collection = new Collection(collectionDb, owner);
            return collection;
        }

        public static Collection MakeCollection(CollectionData collectionData, Hero owner)
        {
            Collection collection = new Collection(collectionData.TemplateId, owner);
            return collection;
        }

        private void SendToClient(CollectionInfo info)
        {
            S_UpdateCollection pkt = new S_UpdateCollection();
            pkt.CollectionInfo = info;
            Owner.Session?.Send(pkt);
        }
        #endregion
    }
}
