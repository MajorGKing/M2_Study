using System.Linq;
using Data;
using Google.Protobuf.Protocol;
using UnityEngine;
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
        GoldText
    }

    enum Sliders
    {
        HpSlider,
        MpSlider,
        ExpSlider
    }

    #endregion

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));
        BindSliders(typeof(Sliders));
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

    }
}