using Data;
using Google.Protobuf.Protocol;
using UnityEngine;

public interface INpcInteraction
{
    public void SetInfo(Npc owner);
    public void HandleOnClickEvent();
    public bool CanInteract();
}

public class Npc : BaseObject
{
    public int TemplateId { get; private set; }
    public NpcData NpcData { get; set; }
    public INpcInteraction Interaction { get; private set; }

    #region LifeCycle
    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void Awake()
    {
        base.Awake();
        // 미니맵
        GameObject icon = Managers.Object.Spawn("MinimapCreatureIcon", transform);
        icon.GetComponent<SpriteRenderer>().color = Color.yellow;
        // TODO 임시, 스케일은 스파인 작업 다 끝나고 일괄처리
        icon.transform.localScale = Vector3.one * 2.04f;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
    }

    public virtual void SetInfo(ObjectInfo info)
    {
        TemplateId = Utils.GetTemplateIdFromId(info.ObjectId);
        ObjectType = Utils.GetObjectTypeFromId(info.ObjectId);
        ObjectId = info.ObjectId;
        PosInfo = info.PosInfo;

        if (Managers.Data.NpcDict.TryGetValue(TemplateId, out NpcData npcData) == false)
            return;
        ExtraCells = npcData.ExtraSize;

        NpcData = npcData;
        SetSpineAnimation(SortingLayers.NPC, "SkeletonAnimation");
        SyncWorldPosWithCellPos();
        SetInteraction();
    }

    private void SetInteraction()
    {
        // TODO UI
        switch (NpcData.NpcType)
        {
            case ENpcType.Portal:
                Interaction = new PortalInteraction();
                break;
            case ENpcType.Shop:
                break;
        }

        Interaction?.SetInfo(this);
    }
    #endregion

    public void OnClickEvent()
    {
        Interaction?.HandleOnClickEvent();
    }
}
