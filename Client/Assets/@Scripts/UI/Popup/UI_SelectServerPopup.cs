using System;
using Data;

public class UI_SelectServerPopup : UI_Popup
{
    enum GameObjects
    {
        Server_1_Button,
        Server_2_Button,
    }

    public Action OnClosed;

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));

        GetObject((int)GameObjects.Server_1_Button).BindEvent((evt) =>
        {
            GameSettingEx.ServerIndex = 1;
            OnClosed?.Invoke();
            ClosePopupUI();
        });

        GetObject((int)GameObjects.Server_2_Button).BindEvent((evt) =>
        {
            GameSettingEx.ServerIndex = 2;
            OnClosed?.Invoke();
            ClosePopupUI();
        });
    }

    public void SetInfo(Action action)
    {
        OnClosed = action;
    }
}
