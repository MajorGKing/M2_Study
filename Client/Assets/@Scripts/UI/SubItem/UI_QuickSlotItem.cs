using Google.Protobuf.Protocol;
using System;
using System.Collections;
using JetBrains.Annotations;
using Scripts.Data;
using Scripts.Data.SO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;
using static UnityEditor.Progress;

public class UI_QuickSlotItem : UI_Base
{
    private enum GameObjects
    {
    }

    private enum Texts
    {
        CountText
    }

    private enum Images
    {
        IconImage,
        CooltimeImage,
        AutoIconImage
    }

    private enum EQuickSlotType
    {
        None,
        Skill,
        Item,
    }

    private EQuickSlotType _slotType = EQuickSlotType.Skill;
    private Skill _skill;
    private Item _item;
    private Image _cooltimeSprite;

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));

        _cooltimeSprite = GetImage((int)Images.CooltimeImage);

        gameObject.BindEvent(OnClickImage);
        GetImage((int)Images.AutoIconImage).gameObject.BindEvent(OnClickAutoIcon);
    }

    public void SetInfo(Skill skill, Item item)
    {
        _skill = skill;
        _item = item;

        if (_skill != null)
        {
            _slotType = EQuickSlotType.Skill;
            GetImage((int)Images.IconImage).sprite = Managers.Resource.Load<Sprite>(_skill.SkillData.IconImage);
        }
        //else if (_item != null)
        //{
        //    _slotType = EQuickSlotType.Item;
        //    GetImage((int)Images.IconImage).sprite = Managers.Resource.Load<Sprite>(_item.TemplateData.IconImage);
        //    GetText((int)Texts.CountText).gameObject.SetActive(true);
        //}
        else
        {
            _slotType = EQuickSlotType.None;
            GetImage((int)Images.IconImage).sprite = null;
            GetText((int)Texts.CountText).gameObject.SetActive(false);
        }

        RefreshUI();
    }

    public void Update()
    {
        RefreshCooltimeUI();
    }

    public void RefreshCooltimeUI()
    {

    }

    public void RefreshUI()
    {

    }

    void OnClickImage(PointerEventData evt)
    {

    }

    void OnClickAutoIcon(PointerEventData evt)
    {
        Debug.Log("OnClickAuto");
    }
}
