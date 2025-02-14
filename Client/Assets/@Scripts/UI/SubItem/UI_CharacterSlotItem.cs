using Google.Protobuf.Protocol;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_CharacterSlotItem : UI_SubItem
{
    private enum GameObjects
    {
    }

    private enum Texts
    {
        CharacterNameText,
        ClassText,
        LevelText,
    }

    private enum Images
    {
        CharacterFrameImage,
        CharacterImage,
        CharacterLevelFrameImage,
        SelectHeroImage
    }

    int _index;
    MyHeroInfo _info;
    bool _selected = false;
    Action<int> _onHeroSelected;

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));

        GetImage((int)Images.SelectHeroImage).gameObject.BindEvent(OnClickSelectHeroImage);
        GetImage((int)Images.SelectHeroImage).gameObject.BindEvent(OnBeginDrag, Define.ETouchEvent.BeginDrag);
        GetImage((int)Images.SelectHeroImage).gameObject.BindEvent(OnDrag, Define.ETouchEvent.Drag);
        GetImage((int)Images.SelectHeroImage).gameObject.BindEvent(OnEndDrag, Define.ETouchEvent.EndDrag);
    }

    public void SetInfo(int index, MyHeroInfo info, bool selected, Action<int> onHeroSelected)
    {
        _index = index;
        _info = info;
        _selected = selected;
        _onHeroSelected = onHeroSelected;
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (_info == null)
            return;

        GetText((int)Texts.CharacterNameText).text = _info.HeroInfo.Name;
        GetText((int)Texts.ClassText).text = _info.HeroInfo.ClassType.ToString();
        GetText((int)Texts.LevelText).text = _info.HeroInfo.Level.ToString();

        if (_selected)
            GetImage((int)Images.SelectHeroImage).color = new Color(0.8f, 0.8f, 0.15f, 0.15f);
        else
            GetImage((int)Images.SelectHeroImage).color = new Color(0.8f, 0.8f, 0.15f, 0);
    }

    void OnClickSelectHeroImage(PointerEventData evt)
    {
        _onHeroSelected?.Invoke(_index);
        RefreshUI();
    }
}
