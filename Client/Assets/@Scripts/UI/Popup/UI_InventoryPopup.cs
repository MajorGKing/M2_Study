using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

public class UI_InventoryPopup : UI_Popup
{
    #region enum

    enum GameObjects
    {
        SlotContainer
    }

    enum Toggles
    {
        AllToggle,
        EquipmentToggle,
        ConsumableToggle,
        ETCToggle
    }

    enum Buttons
    {
        CloseButton,
        DeleteButton
    }

    enum Texts
    {
        InventoryCapacityText,
    }

    #endregion

    List<UI_ItemSlot> _slotItems = new List<UI_ItemSlot>();
    private Toggle _allToggle;
    private Toggle _equipmentToggle;
    private Toggle _consumableToggle;
    private Toggle _etcToggle;
    private int _selectedItemIndex = -1;
    private List<Item> _items = new List<Item>();
    private UI_ItemInfoPopup _itemInfoPopup;
    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindToggles(typeof(Toggles));
        BindTexts(typeof(Texts));
        BindButtons(typeof(Buttons));

        _allToggle = GetToggle((int)Toggles.AllToggle);
        _equipmentToggle = GetToggle((int)Toggles.EquipmentToggle);
        _consumableToggle = GetToggle((int)Toggles.ConsumableToggle);
        _etcToggle = GetToggle((int)Toggles.ETCToggle);

        _allToggle.gameObject.BindEvent(OnClickToggle);
        _equipmentToggle.gameObject.BindEvent(OnClickToggle);
        _consumableToggle.gameObject.BindEvent(OnClickToggle);
        _etcToggle.gameObject.BindEvent(OnClickToggle);

        GetButton((int)Buttons.CloseButton).gameObject.BindEvent(OnClickCloseButton);
        GetButton((int)Buttons.DeleteButton).gameObject.BindEvent(OnClickDelete);

        Transform parent = GetObject((int)GameObjects.SlotContainer).transform;
        parent.DestroyChildren();

        for(int i = 0; i < InventoryManager.DEFAULT_INVENTORY_SLOT_COUNT; i++)
        {
            UI_ItemSlot item = Managers.UI.MakeSubItem<UI_ItemSlot>(parent);
            _slotItems.Add(item);
        }
    }

    private void OnEnable()
    {
        Managers.Event.AddEvent(EEventType.InventoryChanged, RefreshUI);
    }

    private void OnDisable()
    {
        Managers.Event.RemoveEvent(EEventType.InventoryChanged, RefreshUI);
    }

    public void SetInfo()
    {
        RefreshUI();
    }

    void RefreshUI()
    {
        //TODO MAXCOUNT 계산
        int maxItemCount = InventoryManager.DEFAULT_INVENTORY_SLOT_COUNT;
        GetText((int)Texts.InventoryCapacityText).text = $"{Managers.Inventory.ItemCountInInventory} / {maxItemCount}";

        // 아이템 목록 가져오기
        _items = GetItemsBasedOnToggle();

        // 슬롯 아이템 업데이트
        UpdateSlotItems(_items, maxItemCount);
    }

    private List<Item> GetItemsBasedOnToggle()
    {
        if (_equipmentToggle.isOn)
            return Managers.Inventory.GetItemsByGroupTypeInInventory(EItemType.Equipment);

        if (_allToggle.isOn)
            return Managers.Inventory.GetAllItems();

        if(_consumableToggle.isOn)
            return Managers.Inventory.GetItemsByGroupTypeInInventory(EItemType.Consumable);

        return Managers.Inventory.GetEtcItemsInInventory();
    }

    private void UpdateSlotItems(List<Item> items, int maxItemCount)
    {
        for(int i = 0; i < maxItemCount; i++)
        {
            if(i < items.Count)
            {
                _slotItems[i].gameObject.SetActive(true);
                _slotItems[i].SetInfo(i, _selectedItemIndex == i, items[i], OnSelectItem);
            }
            else
            {
                _slotItems[i].gameObject.SetActive(false);
            }
        }
    }

    #region OnClick

    private void OnClickCloseButton(PointerEventData eventData)
    {
        ClosePopupUI();
    }

    private void OnClickDelete(PointerEventData eventData)
    {
        //TODO 확인창
        Item item = _items[_selectedItemIndex];
        Managers.Inventory.ReqDeleteItem(item.ItemDbId);
    }

    private void OnClickToggle(PointerEventData eventData)
    {
        RefreshUI();
    }

    private void OnSelectItem(int index)
    {
        _selectedItemIndex = index;
        RefreshUI();

        if(_itemInfoPopup != null)
        {
            _itemInfoPopup.ClosePopupUI();
            _itemInfoPopup = null;
        }

        _itemInfoPopup = Managers.UI.ShowPopupUI<UI_ItemInfoPopup>();
        _itemInfoPopup.SetInfo(_items[_selectedItemIndex]);
    }
    #endregion
}
