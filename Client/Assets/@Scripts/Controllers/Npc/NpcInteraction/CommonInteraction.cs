using Data.SO;
using Google.Protobuf.Protocol;
using Scripts.Data.SO;

public class CommonInteraction : INpcInteraction
{
    private Npc _owner;
    private NpcData _npcData;

    public void SetInfo(Npc owner)
    {
        _owner = owner;
        if (Managers.Data.NpcDict.TryGetValue(_owner.TemplateId, out _npcData) == false)
            return;
    }

    public void HandleOnClickEvent()
    {
        // 1. Quest Dialogue 확인
        DialogueData dialogueData = Managers.Quest.GetDialogue(EQuestTaskType.InteractWithNpc, _npcData.TemplateId);
        if (dialogueData != null)
        {
            Managers.Game.StartDialogue(dialogueData, SendInteractionPacket, _npcData.IconImage);
            return;
        }

        // 2. Npc가 가지고있는 Dialogue 확인
        Managers.Game.StartDialogue(_npcData.DialogueId, SendInteractionPacket, _npcData.IconImage);

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

    private void SendInteractionPacket()
    {
        C_InteractionNpc pkt = new C_InteractionNpc();
        pkt.ObjectId = _owner.ObjectId;
        Managers.Network.GameServer.Send(pkt);
    }
}
