using Data;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.UI;

public class UI_ObjectHUD : UI_Base
{
    private Creature _owner;
    private Slider _slider;

    [SerializeField]
    private Image _sliderTransitionImage;

    private enum Images
    {
        SliderTransitionImage_Hero,
        SliderTransitionImage_Monster,
        SliderTransitionImage_Boss,
    }

    enum Sliders
    {
        HpBar_Hero,
        HpBar_Monster,
        HpBar_Boss,
    }

    enum Texts
    {
        NameText
    }

    protected override void Awake()
    {

        Bind<Image>(typeof(Images));
        BindTexts(typeof(Texts));
        BindSliders(typeof(Sliders));

        GetComponent<Canvas>().sortingOrder = SortingLayers.HERO + 1;
    }

    public void SetInfo(Creature owner)
    {
        _owner = owner;
        GetText((int)Texts.NameText).gameObject.SetActive(true);

        SetName();
        SetHpBar(true);
        InitSlider();
        transform.localPosition = Vector3.up * (_owner.GetSpineHeight() + 0.5f);

        Refresh(1);
    }

    public void Refresh(float ratio)
    {
        if (_owner.ObjectState == EObjectState.Dead)
        {
            DeactivateAllHpBars();
        }
        else
        {
            _slider.value = ratio;
        }
    }

    private void SetName()
    {
        string name = _owner.GetObjectName();
        if (string.IsNullOrEmpty(name) == true)
        {
            GetText((int)Texts.NameText).gameObject.SetActive(false);
        }
        GetText((int)Texts.NameText).text = name;
    }

    private void InitSlider()
    {
        _slider.value = 1;
        _sliderTransitionImage.fillAmount = 1f;
    }

    public void SetHpBar(bool isInit = false)
    {
        // 모든 HP 바를 비활성화
        DeactivateAllHpBars();

        if (_owner.IsMonitored == false && isInit == false)
            return;

        switch (_owner.ObjectType)
        {
            case EGameObjectType.Hero:
                ActivateHpBar(Sliders.HpBar_Hero, Images.SliderTransitionImage_Hero);
                break;

            case EGameObjectType.Monster:
                if (_owner is Monster monster && Managers.Data.MonsterDict.TryGetValue(monster.TemplateId, out MonsterData monsterData))
                {
                    if (monsterData.IsBoss)
                        ActivateHpBar(Sliders.HpBar_Boss, Images.SliderTransitionImage_Boss);
                    else
                        ActivateHpBar(Sliders.HpBar_Monster, Images.SliderTransitionImage_Monster);
                }
                break;
        }
    }

    public void DeactivateAllHpBars()
    {
        GetSlider((int)Sliders.HpBar_Hero).gameObject.SetActive(false);
        GetSlider((int)Sliders.HpBar_Monster).gameObject.SetActive(false);
        GetSlider((int)Sliders.HpBar_Boss).gameObject.SetActive(false);
    }

    private void ActivateHpBar(Sliders sliderType, Images transitionImageType)
    {
        _slider = GetSlider((int)sliderType);
        _slider.gameObject.SetActive(true);
        _sliderTransitionImage = GetImage((int)transitionImageType);
    }
}
