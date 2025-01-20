using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Google.Protobuf;
using Scripts.Data;
using Scripts.Data.SO;
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
    }

    public void SetInfo()
    {
        RefreshUI();
    }


    private void RefreshUI()
    {
        // TODO : Äü½½·Ô µî·Ï ±â´É.
        List<Skill> skills = Managers.Skill.GetAllSkills(excludeMainSkill: true);

        // ½ºÅ³
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
    }
}
