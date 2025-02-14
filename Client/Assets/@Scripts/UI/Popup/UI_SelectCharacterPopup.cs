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

    // Ä³¸¯ÅÍ ½½·Ô
    Transform _parent;
    List<UI_CharacterSlotItem> _slots = new List<UI_CharacterSlotItem>();

    // µ¥ÀÌÅÍ
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
        // 1) °ÔÀÓ¾À ÀüÈ¯
        // 2) C_EnterGame ÆÐÅ¶ Àü¼Û
        Managers.Game.SelectedHeroIndex = _selectedHeroIndex;
        Managers.Scene.LoadScene(EScene.GameScene);
    }

    void OnClickCreateCharacterButton(PointerEventData evt)
    {
        // 1) Ä³¸¯ÅÍ ÃÖ´ë °³¼ö È®ÀÎ ÈÄ, ¹Ù·Î ÆË¾÷.
        // 2) UI_CreateCharacterPopup¿¡¼­ ³ª¸ÓÁö ÁøÇà.
        // 3) Ä³¸¯ÅÍ »ý¼º ÆË¾÷ ´ÝÈú ¶§, Ä³¸¯ÅÍ ¸ñ·Ï ´Ù½Ã ¿äÃ».
        var popup = Managers.UI.ShowPopupUI<UI_CreateCharacterPopup>();
        popup.SetInfo(onHeroChanged: SendHeroListReqPacket);
    }

    void OnClickDeleteCharacterButton(PointerEventData evt)
    {
        // 1) ÆÐÅ¶ Àü¼Û
        // 2) ´äÀå ¿À¸é Ä³¸¯ÅÍ »èÁ¦ ÈÄ Refresh
        C_DeleteHeroReq reqPacket = new C_DeleteHeroReq();
        reqPacket.HeroIndex = _selectedHeroIndex;
        Managers.Network.Send(reqPacket);
    }

	void OnClickCloseButton(PointerEventData evt)
	{
		// 1) 패킷 전송 (퇴장)
		// 2) 다시 서버 고르는 창으로
		C_LeaveGame leaveGamePacket = new C_LeaveGame();
		Managers.Network.Send(leaveGamePacket);
		ClosePopupUI();
	}

    public void SendHeroListReqPacket()
    {
        C_HeroListReq reqPacket = new C_HeroListReq();
        Managers.Network.Send(reqPacket);
    }

    public void OnDeleteHeroResHandler(S_DeleteHeroRes resPacket)
    {
        if(resPacket.Success)
        {
            _heroes.RemoveAt(resPacket.HeroIndex);
            RefreshUI();
        }
    }
}
