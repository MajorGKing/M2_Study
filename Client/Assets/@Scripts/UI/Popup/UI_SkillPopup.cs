using System.Collections.Generic;
using Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_SkillPopup : UI_Popup
{
    #region enum
    enum GameObjects
    {
        SlotContainer
    }

    enum Toggles
    {
        AllToggle,
        ActiveSkillToggle,
        PassiveSkillToggle,
        SpecialSkillToggle
    }

    enum Buttons
    {
        CloseButton
    }

    enum Texts
    {
    }
    #endregion

    List<UI_SkillSlot> _slotItems = new List<UI_SkillSlot>();
    private Toggle _allToggle;
    private Toggle _activeToggle;
    private Toggle _passiveToggle;
    private Toggle _specialToggle;
    private int _selectedItemIndex = -1;
    private const int SkILL_SLOT_COUNT = 20;

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindToggles(typeof(Toggles));
        BindTexts(typeof(Texts));
        BindButtons(typeof(Buttons));

        _allToggle = GetToggle((int)Toggles.AllToggle);
        _activeToggle = GetToggle((int)Toggles.ActiveSkillToggle);
        _passiveToggle = GetToggle((int)Toggles.PassiveSkillToggle);
        _specialToggle = GetToggle((int)Toggles.SpecialSkillToggle);

        _allToggle.gameObject.BindEvent(OnClickToggle);
        _activeToggle.gameObject.BindEvent(OnClickToggle);
        _passiveToggle.gameObject.BindEvent(OnClickToggle);
        _specialToggle.gameObject.BindEvent(OnClickToggle);
        GetButton((int)Buttons.CloseButton).gameObject.BindEvent(OnClickCloseButton);

        Transform parent = GetObject((int)GameObjects.SlotContainer).transform;
        parent.DestroyChildren();

        for (int i = 0; i < SkILL_SLOT_COUNT; i++)
        {
            UI_SkillSlot item = Managers.UI.MakeSubItem<UI_SkillSlot>(parent);
            _slotItems.Add(item);
        }
    }

    public void SetInfo()
    {
        RefreshUI();
    }

    void RefreshUI()
    {
        // 아이템 목록 가져오기
        List<SkillData> skills = GetSkillBasedOnToggle();

        // 슬롯 아이템 업데이트
        UpdateSlotItems(skills, SkILL_SLOT_COUNT);
    }

    private List<SkillData> GetSkillBasedOnToggle()
    {
        //TODO 일단 모든스킬
        return Managers.Skill.GetAllSkillDatas();
    }

    private void UpdateSlotItems(List<SkillData> skills, int maxItemCount)
    {
        for (int i = 0; i < maxItemCount; i++)
        {
            if (i < skills.Count)
            {
                _slotItems[i].gameObject.SetActive(true);
                _slotItems[i].SetInfo(i, _selectedItemIndex == i, skills[i], OnSelectItem);
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

    private void OnClickToggle(PointerEventData eventData)
    {
        RefreshUI();
    }

    private void OnSelectItem(int index)
    {
        _selectedItemIndex = index;
        RefreshUI();
    }
    #endregion
}
