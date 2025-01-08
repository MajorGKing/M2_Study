using System.Collections;
using Google.Protobuf.Protocol;
using Scripts.Data.SO;
using UnityEngine;
using UnityEngine.UI;

public class UI_HPBar : UI_Base
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

    IEnumerator _slideShowCoroutine;

    protected override void Awake()
    {
        Bind<Image>(typeof(Images));
        BindSliders(typeof(Sliders));

        GetComponent<Canvas>().sortingOrder = SortingLayers.HERO + 1;
    }

    public void SetInfo(Creature owner)
    {
        _owner = owner;

        if (owner.ObjectType == EGameObjectType.Hero)
        {
            GetSlider((int)Sliders.HpBar_Hero).gameObject.SetActive(true);
            GetSlider((int)Sliders.HpBar_Monster).gameObject.SetActive(false);
            GetSlider((int)Sliders.HpBar_Boss).gameObject.SetActive(false);
            _slider = GetSlider((int)Sliders.HpBar_Hero);
            _sliderTransitionImage = GetImage((int)Images.SliderTransitionImage_Hero);
        }
        else
        {
            Monster monster = owner as Monster;
            if (monster == null)
                return;
            if (Managers.Data.MonsterDic.TryGetValue(monster.TemplateId, out MonsterData monsterData) == false)
                return;

            if (monsterData.IsBoss)
            {
                GetSlider((int)Sliders.HpBar_Hero).gameObject.SetActive(false);
                GetSlider((int)Sliders.HpBar_Monster).gameObject.SetActive(false);
                GetSlider((int)Sliders.HpBar_Boss).gameObject.SetActive(true);
                _slider = GetSlider((int)Sliders.HpBar_Boss);
                _sliderTransitionImage = GetImage((int)Images.SliderTransitionImage_Boss);
            }
            else
            {
                GetSlider((int)Sliders.HpBar_Hero).gameObject.SetActive(false);
                GetSlider((int)Sliders.HpBar_Monster).gameObject.SetActive(true);
                GetSlider((int)Sliders.HpBar_Boss).gameObject.SetActive(false);
                _slider = GetSlider((int)Sliders.HpBar_Monster);
                _sliderTransitionImage = GetImage((int)Images.SliderTransitionImage_Monster);
            }
        }

        if (_slideShowCoroutine != null)
        {
            StopCoroutine(_slideShowCoroutine);
            _slideShowCoroutine = null;
        }

        transform.localPosition = Vector3.up * (_owner.GetSpineHeight() * 1.1f);

        _slider.value = 1;
        _sliderTransitionImage.fillAmount = 1f;

        Refresh(1);
    }

    public void Refresh(float ratio)
    {
        if (_owner.ObjectState == EObjectState.Dead)
        {
            _slider.gameObject.SetActive(false);
        }
        else
        {
            _slider.gameObject.SetActive(true);
            _slider.value = ratio;

            if (_slideShowCoroutine == null)
            {
                _slideShowCoroutine = SlideShow();
                StartCoroutine(_slideShowCoroutine);
            }
        }
    }

    IEnumerator SlideShow()
    {
        float decreaseAmount = 0.02f;
        while(true)
        {
            _sliderTransitionImage.fillAmount -= decreaseAmount;
            yield return null;
            if (_sliderTransitionImage.fillAmount < _slider.value)
                break;
        }

        _slideShowCoroutine = null;
    }
}
