using System;
using System.Linq;
using Data;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using static Define;

public class UI_GameScene : UI_Scene
{
    #region Enum
    enum GameObjects
    {
        UI_QuickSlot
    }

    enum Images
    {
        CharacterImage
    }

    enum Buttons
    {
        InventoryButton,
        CharacterButton,
        SkillButton,
    }

    enum Texts
    {
        FpsText,
        LevelText,
        HpText,
        MpText,
        ExpText,
        GoldText,
        AutoText,
        PosValueText,
    }

    enum Sliders
    {
        HpSlider,
        MpSlider,
        ExpSlider
    }

    #endregion

    public UI_QuickSlot QuickSlot { get; set; }
    protected void OnDisable()
    {
        Managers.Event.RemoveEvent(EEventType.CurrencyChanged, RefreshUI);
        Managers.Event.RemoveEvent(EEventType.StatChanged, RefreshUI);
    }

    protected void OnEnable()
    {
        Managers.Event.AddEvent(EEventType.CurrencyChanged, RefreshUI);
        Managers.Event.RemoveEvent(EEventType.StatChanged, RefreshUI);
    }

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));
        BindSliders(typeof(Sliders));

        QuickSlot = GetObject((int)GameObjects.UI_QuickSlot).GetComponent<UI_QuickSlot>();
        GetButton((int)Buttons.InventoryButton).gameObject.BindEvent(OnClickInventory);
        GetButton((int)Buttons.CharacterButton).gameObject.BindEvent(OnClickHeroInfo);
        GetButton((int)Buttons.SkillButton).gameObject.BindEvent(OnClickSkillInfo);
    }

    private float elapsedTime;
    private float updateInterval = 0.3f;

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= updateInterval)
        {
            float fps = 1.0f / Time.deltaTime;
            float ms = Time.deltaTime * 1000.0f;
            string text = string.Format("{0:N1} FPS ({1:N1}ms)", fps, ms);
            // GetText((int)Texts.FpsText).text = text;

            elapsedTime = 0;
        }
    }

    public void SetInfo()
    {
        MyHeroInfo info = Managers.Object.MyHero.MyHeroInfo;
        string iconName = $"HeroIcon_{info.HeroInfo.ClassType}_{info.HeroInfo.Gender}";
        GetImage((int)Images.CharacterImage).sprite = Managers.Resource.Load<Sprite>(iconName);

        RefreshUI();
    }
    
    private void RefreshUI()
    {
        MyHero myHero = Managers.Object.MyHero;
        MyHeroInfo info = Managers.Object.MyHero.MyHeroInfo;

        //·¹º§
        GetText((int)Texts.LevelText).text = info.HeroInfo.Level.ToString();
        GetText((int)Texts.GoldText).text = myHero.Gold.ToString();

        //Hp
        GetSlider((int)Sliders.HpSlider).maxValue = myHero.TotalStat.MaxHp;
        GetSlider((int)Sliders.HpSlider).minValue = 0;
        GetSlider((int)Sliders.HpSlider).value = myHero.Hp;
        GetText((int)Texts.HpText).text = $"{myHero.Hp}/{myHero.TotalStat.MaxHp}";
        //MP
        GetSlider((int)Sliders.MpSlider).maxValue = myHero.TotalStat.MaxMp;
        GetSlider((int)Sliders.MpSlider).minValue = 0;
        GetSlider((int)Sliders.MpSlider).value = myHero.Mp;
        GetText((int)Texts.MpText).text = $"{myHero.Mp}/{myHero.TotalStat.MaxMp}";

        //Exp
        GetSlider((int)Sliders.ExpSlider).maxValue = myHero.GetExpToNextLevel();
        GetSlider((int)Sliders.ExpSlider).minValue = 0;
        GetSlider((int)Sliders.ExpSlider).value = myHero.Exp;
        GetText((int)Texts.ExpText).text = $"{myHero.GetExpNormalized() * 100}%";
    }

    public void OnHpChanged()
    {
        RefreshUI();
    }

    public void OnUpdatePosition()
    {
        MyHero hero = Managers.Object.MyHero;
        GetText((int)Texts.PosValueText).text = $"({hero.CellPos.x} . {hero.CellPos.y})";
    }

    #region Onclick

    private void OnClickInventory(PointerEventData eventData)
    {
        //Managers.UI.ShowToast("TODO OnClickInventory");
        UI_InventoryPopup inven = Managers.UI.ShowPopupUI<UI_InventoryPopup>();
        inven.SetInfo();
    }

    private void OnClickHeroInfo(PointerEventData eventData)
    {
        //Managers.UI.ShowToast("TODO OnClickHeroInfo");
        UI_CharacterPopup characterPopup = Managers.UI.ShowPopupUI<UI_CharacterPopup>();
        characterPopup.SetInfo();
    }

    private void OnClickSkillInfo(PointerEventData eventData)
    {
        //Managers.UI.ShowToast("TODO OnClickSkillInfo");
        UI_SkillPopup skillPopup = Managers.UI.ShowPopupUI<UI_SkillPopup>();
        skillPopup.SetInfo();
    }
    #endregion
}