using System;
using Data;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

public class UI_ItemSlot : UI_SubItem
{
    enum GameObjects
    {
    }

    enum Images
    {
        ItemImage,
        SelectedImage,
        SlotImage
    }

    enum Texts
    {
        ActivateText,
        CountText
    }

    private int _index;
    private UI_InventoryPopup _inventoryPopup;
    private Item _item;
    private ItemData _itemData;
    public bool _isSelected { get; set; }
    TMP_Text _activeText;
    TMP_Text _countText;
    private string _gradeImageName;
    Action<int> _onItemSelected;

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

        _activeText = GetText((int)Texts.ActivateText);
        _countText = GetText((int)Texts.CountText);
    }

    public void SetInfo(int index, bool selected, Item item, Action<int> onItemSelected)
    {
        _index = index;
        _isSelected = selected;
        _item = item;
        _onItemSelected = onItemSelected;
        if (item != null)
            _itemData = item.TemplateData;

        RefreshUI();
    }

    public void RefreshUI()
    {
        RefreshText();
        RefreshSlotImage();
    }

    private void RefreshText()
    {
        _countText.gameObject.SetActive(false);
        _activeText.gameObject.SetActive(false);
        GetImage((int)Images.SelectedImage).gameObject.SetActive(false);

        if (_item == null)
            return;
        switch(_itemData.Type)
        {
            case EItemType.Equipment:
                Item equipped = Managers.Inventory.GetEquippedItem(_item.ItemDbId);
                if (equipped != null && equipped.ItemDbId == _item.ItemDbId)
                {
                    _activeText.text = "@해제";
                }
                else
                {
                    _activeText.text = "@장착";
                }
                break;
            case EItemType.Consumable:
                _countText.gameObject.SetActive(true);
                _activeText.text = "@사용";
                _countText.text = _item.Count.ToString();
                break;
            default:
                _activeText.text = "@사용";
                break;
        }

        if(_isSelected)
        {
            GetImage((int)Images.SelectedImage).gameObject.SetActive(true);
            _activeText.gameObject.SetActive(true);
        }
        else
        {
            GetImage((int)Images.SelectedImage).gameObject.SetActive(false);
            _activeText.gameObject.SetActive(false);
        }
    }

    private void RefreshSlotImage()
    {
        //아이템 이미지
        if (_item != null)
        {
            //등급에 따른 스프라이트 설정
            string slotImageName = String.Empty;
            switch (_itemData.Grade)
            {
                case EItemGrade.Common:
                    slotImageName = "ItemSlot_Common";
                    break;
                case EItemGrade.Uncommon:
                    slotImageName = "ItemSlot_UnCommon";
                    break;
                case EItemGrade.Rare:
                    slotImageName = "ItemSlot_Rare";
                    break;
                case EItemGrade.Epic:
                    slotImageName = "ItemSlot_Epic";
                    break;
                case EItemGrade.Legendary:
                    slotImageName = "ItemSlot_Legendary";
                    break;
                default:
                    slotImageName = "ItemSlot_Common";
                    break;
            }
            GetImage((int)Images.ItemImage).gameObject.SetActive(true);
            GetImage((int)Images.SlotImage).sprite = Managers.Resource.Load<Sprite>(slotImageName);
            GetImage((int)Images.ItemImage).sprite = Managers.Resource.Load<Sprite>(_itemData.IconImage);
        }
        else
        {
            GetImage((int)Images.ItemImage).gameObject.SetActive(false);
            GetImage((int)Images.SlotImage).sprite = Managers.Resource.Load<Sprite>("ItemSlot_Empty");
        }
    }

    #region OnClick
    private void OnClick(PointerEventData eventData)
    {
        if (_isSelected == false)
        {
            _onItemSelected?.Invoke(_index);
            return;
        }

        switch (_itemData.Type)
        {
            case EItemType.Equipment:
                if (Managers.Inventory.IsEquippedItem(_item.ItemDbId))
                    Managers.Inventory.ReqUnEquipItem(_item.ItemDbId);
                else
                    Managers.Inventory.ReqEquipItem(_item.ItemDbId);
                break;
            case EItemType.Consumable:
                //TODO 사용
                // Managers.Inventory.UseItem((_item.InstanceId));
                break;
            default:
                break;
        }
    }

    private void OnLongPressed(PointerEventData eventData)
    {
        Debug.Log("LongPressed");
    }
    #endregion
}
