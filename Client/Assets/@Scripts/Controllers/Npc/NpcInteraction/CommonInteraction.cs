using Data.SO;
using Google.Protobuf.Protocol;
using Scripts.Data.SO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // TODO ILHAK 퀘스트 부분 추가 후 내용 추가
    public void HandleOnClickEvent()
    {
        

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
