using Google.Protobuf.Protocol;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
		// 1) 게임씬 전환
		// 2) C_EnterGame 패킷 전송
		Managers.Game.SelectedHeroIndex = _selectedHeroIndex;
		Managers.Scene.LoadScene(EScene.GameScene);
	}

	void OnClickCreateCharacterButton(PointerEventData evt)
	{
		// 1) 캐릭터 최대 개수 확인 후, 바로 팝업.
		// 2) UI_CreateCharacterPopup에서 나머지 진행.
		// 3) 캐릭터 생성 팝업 닫힐 때, 캐릭터 목록 다시 요청.
		var popup = Managers.UI.ShowPopupUI<UI_CreateCharacterPopup>();
		popup.SetInfo(onHeroChanged: SendHeroListReqPacket);
	}

	void OnClickDeleteCharacterButton(PointerEventData evt)
	{
		// 1) 패킷 전송
		// 2) 답장 오면 캐릭터 삭제 후 Refresh
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
