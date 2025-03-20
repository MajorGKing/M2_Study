using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Game
{
    public class CollectionComponent
    {
        public Dictionary</*TemplateId*/int, Collection> Collections = new Dictionary<int, Collection>();
        public Hero Owner { get; private set; }

        public CollectionComponent(Hero owner)
        {
            Owner = owner;
        }

        public void Init(HeroDb heroDb)
        {
            // DB에 저장된 컬렉션 ADD
            foreach (CollectionDb collection in heroDb.Collections)
                AddCollectionFromDb(collection);

            // 나머지 컬렉션은 미리 만들지 않고, 요청이 오면 만든다.

            // 완료한 컬렉션 보상 이펙트 적용
            ApplyCollectionRewards();
        }

        public Collection FindOrAddCollection(int collectionTemplateId)
        {
            // 이미 진행중이라면 바로 리턴.
            if (Collections.TryGetValue(collectionTemplateId, out Collection collection))
                return collection;

            // 유효한 컬렉션이라면 만들어서 리턴.
            if (DataManager.CollectionDict.TryGetValue(collectionTemplateId, out CollectionData collectionData))
            {
                collection = Collection.MakeCollection(collectionData, Owner);
                Collections.Add(collection.TemplateId, collection);
                return collection;
            }

            return null;
        }

        public void TryFillCollection(int collectionTemplateId, long itemDbId, int index)
        {
            Collection collection = FindOrAddCollection(collectionTemplateId);
            if (collection == null)
                return;

            Item item = Owner.Inven.GetInventoryItemByDbId(itemDbId);
            if (item == null)
                return;

            if (collection.CanCollect(item, index) == false)
                return;

            bool IsFirstCollection = (collection.ProgressFlag == 0);

            // 1. 수집 전 해당 아이템 삭제
            DBManager.DeleteItemNoti(Owner, item);

            // 2. 아이템 등록 진행.
            collection.CollectItem(item, index, sendToClient: true);

            // 3. DB 저장
            if (IsFirstCollection)
                DBManager.AddCollectionNoti(Owner, collection.Info);
            else
                DBManager.SaveCollectionNoti(Owner, collection.Info);
        }

        #region Helper

        public List<CollectionInfo> GetAllInfos()
        {
            List<CollectionInfo> infos = new List<CollectionInfo>();
            foreach (var collection in Collections)
            {
                infos.Add(collection.Value.Info);
            }
            return infos;
        }

        public Collection AddCollectionFromDb(CollectionDb collectionDb)
        {
            Collection collection = Collection.MakeCollection(collectionDb, Owner);
            if (collection == null)
                return null;

            Collections.Add(collection.TemplateId, collection);

            return collection;
        }

        public List<EffectData> GetRewards()
        {
            List<EffectData> effects = new List<EffectData>();

            foreach (var collection in Collections.Values)
            {
                if (collection.IsCompleted && collection.CollectionData.RewardEffectData != null)
                {
                    effects.Add(collection.CollectionData.RewardEffectData);
                }
            }

            return effects;
        }

        public void ApplyCollectionRewards()
        {
            foreach (var effectData in GetRewards())
            {
                // 클라에 따로 보내지 않고 서버에만 적용, 매니저에서 알아서 적용함
                Owner.EffectComp.ApplyEffect(effectData, Owner, false);
            }
        }

        public void Clear()
        {
            Collections.Clear();
        }
        #endregion
    }
}
