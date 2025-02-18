using System;
using System.Collections.Generic;
using System.Linq;
using Data.SO;
using Google.Protobuf.Protocol;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_WorldMapPopup : UI_Popup
{
    enum GameObjects
    {
        Container,
        CloseButton
    }

    List<UI_RoomItem> _roomItems = new List<UI_RoomItem>();
    private int _selectedRoomIndex = -1;
    private const int MAX_COUNT = 30;
    Action<bool> _onClosePopup;
    List<RoomData> _roomDatas = new List<RoomData>();

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        GetObject((int)GameObjects.CloseButton).gameObject.BindEvent(OnClickCloseButton);

        Transform parent = GetObject((int)GameObjects.Container).transform;
        parent.DestroyChildren();

        for(int i = 0; i < MAX_COUNT; i++)
        {
            UI_RoomItem item = Managers.UI.MakeSubItem<UI_RoomItem>(parent);
            _roomItems.Add(item);
        }
    }

    public void SetInfo()
    {
        _roomDatas = Managers.Data.RoomDict.Values.ToList();
        RefreshUI();
    }

    public void RefreshUI()
    {
        for (int i = 0; i < MAX_COUNT; i++)
        {
            if (i < Managers.Data.RoomDict.Count)
            {
                _roomItems[i].gameObject.SetActive(true);
                _roomItems[i].SetInfo(i, _selectedRoomIndex == i, _roomDatas[i], OnSelectItem);
            }
            else
            {
                _roomItems[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnClickCloseButton(PointerEventData eventData)
    {
        ClosePopupUI();
    }

    private void OnSelectItem(int index)
    {
        _selectedRoomIndex = index;
        RoomData roomData = _roomDatas[_selectedRoomIndex];
        Managers.UI.ShowMessage(Define.MsgPopupType.YesNo, $"{roomData.MapName}\n텔레포트 하시겠습니까?", () =>
        {
            C_ReqTeleport reqTeleport = new C_ReqTeleport();
            reqTeleport.PosInfo = new PositionInfo();
            reqTeleport.PosInfo.RoomId = roomData.TemplateId;
            reqTeleport.PosInfo.PosX = roomData.StartPosX;
            reqTeleport.PosInfo.PosY = roomData.StartPosY;

            Managers.Network.Send(reqTeleport);
            ClosePopupUI();
        });
    }
}
