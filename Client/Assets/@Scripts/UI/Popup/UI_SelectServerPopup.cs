using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_SelectServerPopup : UI_Popup
{
    enum GameObjects
    {
        Server_1_Button,
        Server_2_Button,
    }

    public Action<int> OnClosed;

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));

        GetObject((int)GameObjects.Server_1_Button).BindEvent((evt) =>
        {
            OnClosed?.Invoke(0);
            ClosePopupUI();
        });

        GetObject((int)GameObjects.Server_2_Button).BindEvent((evt) =>
        {
            OnClosed?.Invoke(1);
            ClosePopupUI();
        });
    }

    public void SetInfo(Action<int> action)
    {
        OnClosed = action;
    }
}
