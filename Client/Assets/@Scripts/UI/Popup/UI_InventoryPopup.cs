using System;
using System.Collections.Generic;
using Data;
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
}
