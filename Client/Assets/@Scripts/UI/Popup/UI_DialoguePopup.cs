using Scripts.Contents.Dialog;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_DialoguePopup : UI_Popup
{
    enum GameObjects
    {
        CloseArea,
    }

    enum Buttons
    {
        NextButton,
        SkipButton,
    }

    enum Images
    {
        OwnerIcon,
        PlayerIcon,
    }

    enum Texts
    {
        NameText,
        DialogueText
    }

    private MyHero _myHero;

    protected override void Awake()
    {
        base.Awake();
        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindImages(typeof(Images));
        BindTexts(typeof(Texts));

        GetButton((int)Buttons.NextButton).gameObject.BindEvent(OnClickNextButton);
        GetButton((int)Buttons.SkipButton).gameObject.BindEvent(OnClickSkipButton);
    }

    public void SetInfo(string ownerIconName)
    {
        GetImage((int)Images.PlayerIcon).sprite = Managers.Resource.Load<Sprite>(Managers.Object.MyHero.GetIconName());
        GetImage((int)Images.OwnerIcon).sprite = Managers.Resource.Load<Sprite>(ownerIconName);

        Refresh();
    }

    void Refresh()
    {
        DialogueHandler dialogueHandler = Managers.Object.MyHero.DialogueHandler;
        GetImage((int)Images.OwnerIcon).gameObject.SetActive(!dialogueHandler.IsPlayerSpeaking());
        GetImage((int)Images.PlayerIcon).gameObject.SetActive(dialogueHandler.IsPlayerSpeaking());

        GetText((int)Texts.NameText).text = dialogueHandler.GetCurrentConversantName();
        GetText((int)Texts.DialogueText).text = dialogueHandler.GetText();
    }


    void OnClickSkipButton(PointerEventData eventData)
    {
        DialogueHandler dialogueHandler = Managers.Object.MyHero.DialogueHandler;
        if (dialogueHandler.HasNext() == false)
        {
            dialogueHandler.Quit();
            ClosePopupUI();
        }
        else
        {
            Managers.Object.MyHero.DialogueHandler.Skip();
            Refresh();
        }
    }

    void OnClickNextButton(PointerEventData eventData)
    {
        DialogueHandler dialogueHandler = Managers.Object.MyHero.DialogueHandler;

        if (dialogueHandler.HasNext())
        {
            dialogueHandler.Next();
            Refresh();
        }
        else
        {
            dialogueHandler.Quit();
            ClosePopupUI();
        }
    }
}
