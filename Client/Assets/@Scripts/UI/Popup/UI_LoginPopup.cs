using System;
using UnityEngine.EventSystems;

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

        // TODO : 인증서버를 통해 로그인.
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
        // 1) TODO : 인증서버로 인증 요청
        // 2) TODO : 인증서버에서 인증 성공하면, AccountDbId 및 JWT 받아와서 이어서 진행.

        // TEMP
        Managers.AccountDbId = 0;
        Managers.Jwt = "";

        _onClosePopup?.Invoke(true);
        ClosePopupUI();
    }
}
