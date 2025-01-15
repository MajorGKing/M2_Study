using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Google.Protobuf;
using Scripts.Data;
using Scripts.Data.SO;
using UnityEngine;
using static Define;

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
}
