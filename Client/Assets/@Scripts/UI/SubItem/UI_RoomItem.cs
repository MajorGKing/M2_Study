using System;
using Data.SO;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

public class UI_RoomItem : UI_SubItem
{
    enum GameObjects
    {
        CheckImage,
    }

    enum Buttons
    {
        RoomButton,
    }

    enum Images
    {
    }

    enum Texts
    {
        RoomNameText,
    }

    private int _index;
    private UI_WorldMapPopup _inventoryPopup;
    private RoomData _roomData;

    public bool _isSelected { get; set; }
    Action<int> _onRoomSelected;

    protected override void Awake()
    {
        base.Awake();
        BindObjects(typeof(GameObjects));
        BindImages(typeof(Images));
        BindTexts(typeof(Texts));

        gameObject.BindEvent(OnClick);
        gameObject.BindEvent(OnBeginDrag, ETouchEvent.BeginDrag);
        gameObject.BindEvent(OnDrag, ETouchEvent.Drag);
        gameObject.BindEvent(OnEndDrag, ETouchEvent.EndDrag);
        gameObject.BindEvent(OnLongPressed, ETouchEvent.LongPressed);
    }

    public void SetInfo(int index, bool selected, RoomData rooomData, Action<int> onRoomSelected)
    {
        _index = index;
        _isSelected = selected;
        _roomData = rooomData;
        _onRoomSelected = onRoomSelected;

        GetText((int)Texts.RoomNameText).text = $"{_roomData.TemplateId}. {_roomData.MapName}";
        RefreshUI();
    }

    public void RefreshUI()
    {
        GetObject((int)GameObjects.CheckImage).SetActive(false);

        if (_isSelected)
            GetObject((int)GameObjects.CheckImage).SetActive(true);
    }

    #region OnClick

    private void OnClick(PointerEventData eventData)
    {
        _onRoomSelected?.Invoke(_index);
    }

    private void OnLongPressed(PointerEventData eventData)
    {
        Debug.Log("LongPressed");
    }


    #endregion
}
