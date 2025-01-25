using System;
using System.Collections.Generic;
using Data;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

public class UI_ItemInfoPopup : UI_Popup
{
    #region enum

    enum GameObjects
    {
        LockedImage,
        UnlockedImage,
        ItemInfoList
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
        ItemNameText,
        ItemTypeText,
        GradeInfoText,
        DescriptionInfoText,
        CoolTimeInfoText,
        StackInfotext,

        OptionInfoText1,
        OptionInfoText2,
        OptionInfoText3,
        OptionInfoText4,
        OptionInfoText5,
    }

    #endregion

    private Item _item;

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindToggles(typeof(Toggles));
        BindTexts(typeof(Texts));
        BindButtons(typeof(Buttons));

        GetButton((int)Buttons.CloseButton).gameObject.BindEvent(OnClickCloseButton);
        GetButton((int)Buttons.DeleteButton).gameObject.BindEvent(OnClickDelete);
    }

    private void OnEnable()
    {
        Managers.Event.AddEvent(EEventType.InventoryChanged, RefreshUI);
    }

    private void OnDisable()
    {
        Managers.Event.RemoveEvent(EEventType.InventoryChanged, RefreshUI);
    }

    public void SetInfo(Item item)
    {
        _item = item;
        RefreshUI();
    }

    void RefreshUI()
    {
        GetObject((int)GameObjects.UnlockedImage).SetActive(true);
        GetObject((int)GameObjects.LockedImage).SetActive(false);
        GetText((int)Texts.StackInfotext).gameObject.SetActive(false);
        GetText((int)Texts.CoolTimeInfoText).gameObject.SetActive(false);

        //�������̸�
        GetText((int)Texts.ItemNameText).text = _item.TemplateData.NameTextId;
        //����������
        GetText((int)Texts.ItemTypeText).text = $"@@{_item.TemplateData.SubType}";
        //������ ���
        GetText((int)Texts.GradeInfoText).text = $"@@{_item.TemplateData.Grade}";
        //������
        GetText((int)Texts.DescriptionInfoText).text = $"@@{_item.TemplateData.DescriptionTextID}";
        //�ɼ�
        switch (_item.TemplateData.Type)
        {
            case EItemType.Equipment:
                EquipmentData equipmentData = _item.TemplateData as EquipmentData;
                //TODO �ϵ��ڵ�
                for (int i = 0; i < 5; i++)
                {
                    GetText(((int)Texts.OptionInfoText1 + i)).gameObject.SetActive(false);
                }

                if (equipmentData.EffectData == null)
                    break;

                for (int i = 0; i < equipmentData.EffectData.StatValues.Count; i++)
                {
                    EStatType statType = equipmentData.EffectData.StatValues[i].StatType;
                    float value = equipmentData.EffectData.StatValues[i].AddValue;

                    GetText((int)Texts.OptionInfoText1 + i).gameObject.SetActive(true);
                    GetText((int)Texts.OptionInfoText1 + i).text = $"@@{statType} +{value}";
                }
                break;
            case EItemType.Consumable:
                GetText((int)Texts.StackInfotext).gameObject.SetActive(true);
                GetText((int)Texts.CoolTimeInfoText).gameObject.SetActive(true);

                ConsumableData consumableData = _item.TemplateData as ConsumableData;
                GetText((int)Texts.StackInfotext).text = $"@@{consumableData.MaxStack}��";
                GetText((int)Texts.CoolTimeInfoText).text = $"@@{consumableData.CoolTime}��";
                break;
        }

        //�׸��� �ʱ�ȭ
        ContentSizeFitter csf = GetObject((int)GameObjects.ItemInfoList).GetComponent<ContentSizeFitter>();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)csf.transform);
    }

    #region OnClick

    private void OnClickCloseButton(PointerEventData eventData)
    {
        ClosePopupUI();
    }

    private void OnClickDelete(PointerEventData eventData)
    {
        //TODO Ȯ��â
        Managers.Inventory.ReqDeleteItem(_item.ItemDbId);
    }


    #endregion
}