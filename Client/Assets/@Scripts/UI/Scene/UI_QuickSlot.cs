using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Google.Protobuf;
using UnityEngine;
using static Define;
using static UnityEditor.Progress;

public class UI_QuickSlot : UI_Base
{
    #region Enum
    enum GameObjects
    {
        CenterContainer,
        RightContainer
    }

    enum Images
    {

    }

    enum Buttons
    {

    }

    enum Texts
    {
    }

    enum Sliders
    {

    }

    #endregion

    private Transform _centerContainer;
    private Transform _rightContainer;
    private List<UI_QuickSlotItem> _skillSlots = new List<UI_QuickSlotItem>();
    private List<UI_QuickSlotItem> _itemSlots = new List<UI_QuickSlotItem>();

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));
        BindSliders(typeof(Sliders));

        _centerContainer = GetObject((int)GameObjects.CenterContainer).transform;
        _rightContainer = GetObject((int)GameObjects.RightContainer).transform;
        PopulateSlots();
        RefreshUI();
    }

    private void OnEnable()
    {
        Managers.Event.AddEvent(EEventType.InventoryChanged, RefreshUI);
    }

    private void OnDisable()
    {
        Managers.Event.RemoveEvent(EEventType.InventoryChanged, RefreshUI);
    }

    void PopulateSlots()
    {
        _centerContainer.DestroyChildren();
        _rightContainer.DestroyChildren();
        _skillSlots.Clear();

        for (int i = 0; i < MAX_QUICKSLOT_COUNT; i++)
        {
            UI_QuickSlotItem item = Managers.UI.MakeSubItem<UI_QuickSlotItem>(_centerContainer);
            _skillSlots.Add(item);
        }

        for (int i = 0; i < MAX_QUICKSLOT_COUNT; i++)
        {
            UI_QuickSlotItem item = Managers.UI.MakeSubItem<UI_QuickSlotItem>(_rightContainer);
            _itemSlots.Add(item);
        }
    }

    public void SetInfo()
    {
        RefreshUI();
    }


    private void RefreshUI()
    {
        // TODO : 퀵슬롯 등록 기능.
        List<Skill> skills = Managers.Skill.GetAllSkills(excludeMainSkill: true);

        // 스킬
        for (int i = 0; i < MAX_QUICKSLOT_COUNT; i++)
        {
            if (i < skills.Count)
            {
                Skill skill = skills[i];
                _skillSlots[i].SetInfo(skill, null);
            }
            else
            {
                _skillSlots[i].SetInfo(null, null);
            }
        }

        // 아이템
        for (int i = 0; i < MAX_QUICKSLOT_COUNT; i++)
        {
            if (i < Managers.Inventory.QuickSlotItems.Count)
            {
                Item item = Managers.Inventory.QuickSlotItems[i];

                _itemSlots[i].gameObject.SetActive(true);
                _itemSlots[i].SetInfo(null, item);
            }
            else
            {
                _itemSlots[i].gameObject.SetActive(false);
            }
        }
    }
}
