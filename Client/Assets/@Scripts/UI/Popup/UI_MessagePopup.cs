using System;
using UnityEngine.EventSystems;

public class UI_MessagePopup : UI_Popup
{
    enum Texts
    {
        MessageText
    }

    enum GameObjects
    {
        YesNo,
        Yes,
        PositiveButton,
        NegativeButton,
        YesButton,
    }

    Action _onReply;

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindTexts(typeof(Texts));

        GetObject((int)GameObjects.PositiveButton).BindEvent(OnClickPositive);
        GetObject((int)GameObjects.NegativeButton).BindEvent(ClosePopup);
        GetObject((int)GameObjects.YesButton).BindEvent(ClosePopup);
    }

    public void ShowMessage(Define.MsgPopupType msgPopupType, string Message, Action replyAction)
    {
        _onReply = replyAction;
        GetText((int)Texts.MessageText).text = Message;
        GetObject((int)GameObjects.YesNo).SetActive(false);
        GetObject((int)GameObjects.Yes).SetActive(false);

        switch (msgPopupType)
        {
            case Define.MsgPopupType.Yes:
                GetObject((int)GameObjects.Yes).SetActive(true);
                break;
            case Define.MsgPopupType.YesNo:
                GetObject((int)GameObjects.YesNo).SetActive(true);
                break;
        }
    }

    void OnClickPositive(PointerEventData evt)
    {
        _onReply?.Invoke();
        ClosePopupUI();
    }

    void ClosePopup(PointerEventData evt)
    {
        ClosePopupUI();
    }
}
