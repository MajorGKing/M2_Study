using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        else if (_item != null)
        {
            _slotType = EQuickSlotType.Item;
            GetImage((int)Images.IconImage).sprite = Managers.Resource.Load<Sprite>(_item.TemplateData.IconImage);
            GetText((int)Texts.CountText).gameObject.SetActive(true);
        }
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
            case EQuickSlotType.Item:
                {
                    float targetFillAmount = Managers.Inventory.GetRemainingCoolTimeRatio(_item.Info.ItemDbId);
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
            case EQuickSlotType.Item:
                GetText((int)Texts.CountText).gameObject.SetActive(true);
                GetText((int)Texts.CountText).text = _item.Count.ToString();
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
            case EQuickSlotType.Item:
                TryUseItem();
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

        Creature target = hero.GetSkillTarget(_skill);
        if (target == null)
        {
            Managers.UI.ShowToast("TODO 타겟이 없습니다.");
            return;
        }

        ECanUseSkillFailReason reason = _skill.CanUseSkill(target);
        if (reason == ECanUseSkillFailReason.None)
        {
            if (_skill.SkillData.UseSkillTargetType != EUseSkillTargetType.Self)
                hero.Target = target;
            hero.ReqUseSkill(_skill.TemplateId);
        }
        else
        {
            switch (reason)
            {
                case ECanUseSkillFailReason.InvalidTarget:
                    Managers.UI.ShowToast("TODO 실패 타겟이 없습니다.");
                    return;
                case ECanUseSkillFailReason.Cooltime:
                    Managers.UI.ShowToast("TODO 실패 아직 사용 할 수 없습니다.");
                    return;
                case ECanUseSkillFailReason.SkillCost:
                    Managers.UI.ShowToast("TODO 실패 마나가 부족합니다.");
                    return;
                case ECanUseSkillFailReason.SkillRange:
                    Managers.UI.ShowToast("TODO 실패 너무 멀리 있습니다.");
                    return;
            }
        }
    }

    #endregion

    #region Item
    void TryUseItem()
    {
        if (_item == null)
            return;

        MyHero hero = Managers.Object.MyHero;
        if (hero == null)
            return;

        if (Managers.Inventory.CanUseItem(_item.ItemDbId))
        {
            Managers.Inventory.ReqUseItem(_item);
        }
        else
        {
            Managers.UI.ShowToast("TODO 아직 사용 할 수 없습니다.");
        }
    }
    #endregion
}
