using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_LoginPopup : UI_Popup
{
    enum GameObjects
    {
        GoogleLoginButton,
        AppleLoginButton,
        GuestLoginButton,
    }

    Action<bool> _onClosePopup;

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));

        // TODO : ���������� ���� �α���.
        GetObject((int)GameObjects.GoogleLoginButton).BindEvent(OnClickLogginButton);
        GetObject((int)GameObjects.AppleLoginButton).BindEvent(OnClickLogginButton);
        GetObject((int)GameObjects.GuestLoginButton).BindEvent(OnClickLogginButton);
    }

    public void SetInfo(Action<bool> action)
    {
        _onClosePopup = action;
    }

    void OnClickLogginButton(PointerEventData evt)
    {
        // 1) TODO : ���������� ���� ��û
        // 2) TODO : ������������ ���� �����ϸ�, AccountDbId �� JWT �޾ƿͼ� �̾ ����.

        // TEMP
        Managers.AccountDbId = 0;
        Managers.Jwt = "";

        _onClosePopup?.Invoke(true);
        ClosePopupUI();
    }
}
