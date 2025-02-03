using Data.SO;
using Google.Protobuf.Protocol;

public class PortalInteraction : INpcInteraction
{
    private Npc _owner;
    private PortalData _portalData;

    public void SetInfo(Npc owner)
    {
        _owner = owner;
        if (Managers.Data.PortalDict.TryGetValue(_owner.TemplateId, out _portalData) == false)
            return;
    }

    public void HandleOnClickEvent()
    {
        C_InteractionNpc pkt = new C_InteractionNpc();
        pkt.ObjectId = _owner.ObjectId;
        Managers.Network.GameServer.Send(pkt);
    }

    public bool CanInteract()
    {
        if (_owner.GetDistance(Managers.Object.MyHero) < 5)
        {
            return true;
        }
        else
        {
            Managers.UI.ShowToast("@@ 너무 멀리 있습니다.");
            return false;
        }
    }
}
