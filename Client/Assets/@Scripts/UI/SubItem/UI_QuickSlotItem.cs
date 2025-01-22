using Google.Protobuf.Protocol;
using System;
using System.Collections;
using JetBrains.Annotations;
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
        switch (_slotType)
        {
            case EQuickSlotType.Skill:
                {
                    float targetFillAmount = Managers.Skill.GetRemainingCoolTimeRatio(_skill.TemplateId);
                    _cooltimeSprite.fillAmount = targetFillAmount;
                }
                break;
        }
    }

    public void RefreshUI()
    {
        switch (_slotType)
        {
            case EQuickSlotType.Skill:
                GetText((int)Texts.CountText).gameObject.SetActive(false);
                break;
            default:
                GetText((int)Texts.CountText).gameObject.SetActive(false);
                break;
        }

        RefreshCooltimeUI();
    }

    void OnClickImage(PointerEventData evt)
    {
        switch (_slotType)
        {
            case EQuickSlotType.Skill:
                TryUseSkill();
                break;
        }
    }

    void OnClickAutoIcon(PointerEventData evt)
    {
        Debug.Log("OnClickAuto");
    }

    #region Skill
    void TryUseSkill()
    {
        if (_skill == null)
            return;

        MyHero hero = Managers.Object.MyHero;
        if (hero == null)
            return;

        Creature target = hero.GetSelectedTarget();
        if (target == null)
        {
            Managers.UI.ShowToast("TODO 타겟이 없습니다.");
            return;
        }

        ECanUseSkillFailReason reason = _skill.CanUseSkill(target);
        if (reason == ECanUseSkillFailReason.None)
        {
            hero.Target = target;
            hero.ReqUseSkill(_skill.TemplateId);
        }
        else
        {
            switch (reason)
            {
                case ECanUseSkillFailReason.InvalidTarget:
                    Managers.UI.ShowToast("TODO 타겟이 없습니다.");
                    return;
                case ECanUseSkillFailReason.Cooltime:
                    Managers.UI.ShowToast("TODO 아직 사용 할 수 없습니다.");
                    return;
                case ECanUseSkillFailReason.SkillCost:
                    Managers.UI.ShowToast("TODO 마나가 부족합니다.");
                    return;
                case ECanUseSkillFailReason.SkillRange:
                    Managers.UI.ShowToast("TODO 너무 멀리 있습니다.");
                    return;
            }
        }
    }

    #endregion
}
