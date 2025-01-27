using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using Google.Protobuf.Protocol;
using UnityEngine.UI;

public class UI_CharacterPopup : UI_Popup
{
    #region enum
    enum GameObjects
    {
        EquipmentContainer,
        EquipmentPage,
        StatPage
    }

    enum Toggles
    {
        EquipmentToggle,
        StatToggle
    }

    enum Buttons
    {
        CloseButton,
    }

    enum Texts
    {
        STRValueText,
        INTValueText,
        DEXValueText,
        WISValueText,
        CONValueText,
        // CHAValueText
    }
    #endregion

    private Toggle _equipmentToggle;
    private Toggle _statToggle;

    private Dictionary<EItemSlotType, UI_ItemSlot> _equippedItems = new Dictionary<EItemSlotType, UI_ItemSlot>();
    private EItemSlotType _selectedItemSlotType = EItemSlotType.None;

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindToggles(typeof(Toggles));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));

        _statToggle = GetToggle((int)Toggles.StatToggle);
        _equipmentToggle = GetToggle((int)Toggles.EquipmentToggle);
        _statToggle.gameObject.BindEvent(OnClickToggle);
        _equipmentToggle.gameObject.BindEvent(OnClickToggle);

        GetButton((int)Buttons.CloseButton).gameObject.BindEvent(OnClickCloseButton);

        Transform parent = GetObject((int)GameObjects.EquipmentContainer).transform;
        parent.DestroyChildren();

        // ΩΩ∑‘ √ ±‚»≠
        for (EItemSlotType slotType = EItemSlotType.None; slotType < EItemSlotType.EquipmentMax; slotType++)
        {
            UI_ItemSlot item = Managers.UI.MakeSubItem<UI_ItemSlot>(parent);
            _equippedItems.Add(slotType, item);
        }
    }

    private void OnEnable()
    {
        Managers.Event.AddEvent(Define.EEventType.InventoryChanged, RefreshUI);
    }

    private void OnDisable()
    {
        Managers.Event.RemoveEvent(Define.EEventType.InventoryChanged, RefreshUI);
    }

    public void SetInfo()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        GetObject((int)GameObjects.EquipmentPage).SetActive(false);
        GetObject((int)GameObjects.StatPage).SetActive(false);

        if (_statToggle.isOn)
        {
            GetObject((int)GameObjects.StatPage).SetActive(true);
            RefreshStat();
        }

        if (_equipmentToggle.isOn)
        {
            GetObject((int)GameObjects.EquipmentPage).SetActive(true);
            RefreshEquipment();
        }
    }

    private void RefreshEquipment()
    {
        foreach (var pair in _equippedItems)
        {
            Item equipment = Managers.Inventory.GetEquippedItem(pair.Key);
            pair.Value.SetInfo((int)pair.Key, _selectedItemSlotType == pair.Key, equipment, OnSelectEquipment);
        }
    }

    private void RefreshStat()
    {
        MyHero myHero = Managers.Object.MyHero;
        GetText((int)Texts.STRValueText).text = myHero.TotalStat.Str.ToString();
        GetText((int)Texts.INTValueText).text = myHero.TotalStat.Int.ToString();
        GetText((int)Texts.DEXValueText).text = myHero.TotalStat.Dex.ToString();
        GetText((int)Texts.WISValueText).text = myHero.TotalStat.Wis.ToString();
        GetText((int)Texts.CONValueText).text = myHero.TotalStat.Con.ToString();
    }

    private void OnClickToggle(PointerEventData eventData)
    {
        RefreshUI();
    }

    void OnClickCloseButton(PointerEventData evt)
    {
        ClosePopupUI();
    }

    private void OnSelectEquipment(int selectedId)
    {
        _selectedItemSlotType = (EItemSlotType)selectedId;
        RefreshUI();
    }
}
