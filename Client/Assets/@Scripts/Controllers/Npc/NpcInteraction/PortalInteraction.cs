using Data.SO;
using Google.Protobuf.Protocol;

public class PortalInteraction : INpcInteraction
{
    private Npc _owner;
    private PortalData _portalData;

    public bool CanInteract()
    {
        throw new System.NotImplementedException();
    }

    public void HandleOnClickEvent()
    {
        throw new System.NotImplementedException();
    }

    public void SetInfo(Npc owner)
    {
        _owner = owner;
        if (Managers.Data.PortalDict.TryGetValue(_owner.TemplateId, out _portalData) == false)
            return;
    }


}
