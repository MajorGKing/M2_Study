using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

public class UI_SelectCharacterPopup : UI_Popup
{
    enum GameObjects
    {
        StartButton,
        CreateCharacterButton,
        DeleteCharacterButton,
        CloseButton,
        CharacterList,
    }

    Transform _parent;
    List<UI_CharacterSlotItem> _slots = new List<UI_CharacterSlotItem>();

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));

        GetObject((int)GameObjects.StartButton).BindEvent(OnClickStartButton);
        GetObject((int)GameObjects.CreateCharacterButton).BindEvent(OnClickCreateCharacterButton);
        GetObject((int)GameObjects.DeleteCharacterButton).BindEvent(OnClickDeleteCharacterButton);
        GetObject((int)GameObjects.CloseButton).BindEvent(OnClickCloseButton);

        _parent = GetObject((int)GameObjects.CharacterList).transform;

        PopulateSlots();
        RefreshUI();
    }

    void PopulateSlots()
    {

    }

    public void RefreshUI()
    {

    }

    void OnClickStartButton(PointerEventData evt)
    {

    }

    void OnClickCreateCharacterButton(PointerEventData evt)
    {

    }

    void OnClickDeleteCharacterButton(PointerEventData evt)
    {

    }

    void OnClickCloseButton(PointerEventData evt)
    {

    }

    public void SendHeroListReqPacket()
    {
        C_HeroListReq reqPacket = new C_HeroListReq();
        Managers.Network.Send(reqPacket);
    }

}
