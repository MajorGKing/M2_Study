using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VFolders.Libs;
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

    // 캐릭터 슬롯
    Transform _parent;
    List<UI_CharacterSlotItem> _slots = new List<UI_CharacterSlotItem>();

    // 데이터
    List<MyHeroInfo> _heroes = new List<MyHeroInfo>();

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

    public void SetInfo(List<MyHeroInfo> heroes)
    {
        _heroes = heroes;

        RefreshUI();
    }

    void PopulateSlots()
    {
        _parent.DestroyChildren();
        _slots.Clear();

        for (int i = 0; i < MAX_LOBBY_HERO_COUNT; i++)
        {
            UI_CharacterSlotItem item = Managers.UI.MakeSubItem<UI_CharacterSlotItem>(_parent);
            item.gameObject.SetActive(false);
            _slots.Add(item);
        }
    }

    int _selectedHeroIndex = 0;

    public void RefreshUI()
    {
        for (int i = 0; i < MAX_LOBBY_HERO_COUNT; i++)
        {
            if (i < _heroes.Count)
            {
                MyHeroInfo myHeroInfo = _heroes[i];

                _slots[i].SetInfo(i, myHeroInfo, _selectedHeroIndex == i, OnHeroSelected);
                _slots[i].gameObject.SetActive(true);
            }
            else
            {
                _slots[i].gameObject.SetActive(false);
            }
        }
    }

    void OnHeroSelected(int index)
    {
        _selectedHeroIndex = index;
        RefreshUI();
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
