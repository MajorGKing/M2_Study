using System;
using Data;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_SkillSlot : UI_SubItem
{
    #region enum
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
    }
    #endregion

    public bool _isSelected { get; set; }
    private int _index;
    private UI_SkillPopup _skillPopup;
    private SkillData _skillData;
    private string _gradeImageName;
    private Action<int> _onItemSelected;

    protected override void Awake()
    {
        BindObjects(typeof(GameObjects));
        BindImages(typeof(Images));
        BindTexts(typeof(Texts));

        gameObject.BindEvent(OnClick);
    }

    public void SetInfo(int index, bool selected, SkillData skillData, Action<int> onItemSelected)
    {
        _index = index;
        _isSelected = selected;
        _skillData = skillData;
        _onItemSelected = onItemSelected;
        RefreshUI();
    }

    public void RefreshUI()
    {
        RefreshText();
        RefreshSlotImage();
    }

    private void RefreshText()
    {
        GetImage((int)Images.SelectedImage).gameObject.SetActive(false);

        if (_skillData == null)
            return;

        //TODO 사용/미사용
        if (_isSelected)
        {
            GetImage((int)Images.SelectedImage).gameObject.SetActive(true);
        }
        else
        {
            GetImage((int)Images.SelectedImage).gameObject.SetActive(false);
        }
    }

    private void RefreshSlotImage()
    {
        //아이템 이미지
        if (_skillData != null)
        {
            //등급에 따른 스프라이트 설정
            string slotImageName = String.Empty;
            switch (_skillData.SkillGrade)
            {
                case EItemGrade.Common:
                    slotImageName = "ItemSlot_Common";
                    break;
                case EItemGrade.Uncommon:
                    slotImageName = "ItemSlot_Uncommon";
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
            GetImage((int)Images.ItemImage).sprite = Managers.Resource.Load<Sprite>(_skillData.IconImage);
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
        Debug.Log("Click Item");

    }
    #endregion
}
