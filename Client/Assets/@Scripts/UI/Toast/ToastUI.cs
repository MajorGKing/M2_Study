using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class ToastUI : UI_Base
{
    #region Enums

    enum GameObjects
    {
        Canvas,
        Container,
        Toast,
    }

    enum Texts
    {
        MsgText
    }

    #endregion

    private CanvasGroup _uiCanvasGroup;
    private VerticalLayoutGroup _container;
    private Image _uiImage;
    private TMP_Text _uiText;

    [Header("Toast Colors :")]
    [SerializeField] private Color[] colors;

    [Header("Toast Fade In/Out Duration :")]
    [Range(.1f, .8f)]
    [SerializeField] private float fadeDuration = .3f;

    private int maxTextLength = 300;

    protected override void Awake()
    {
        base.Awake();

        #region Object Bind
        BindObjects(typeof(GameObjects));
        BindTexts(typeof(Texts));
        #endregion

        _uiCanvasGroup = GetObject((int)GameObjects.Canvas).GetComponent<CanvasGroup>();
        _container = GetObject((int)GameObjects.Container).GetComponent<VerticalLayoutGroup>();
        _uiImage = GetObject((int)GameObjects.Toast).GetComponent<Image>();
        _uiText = GetText((int)Texts.MsgText);

        _uiCanvasGroup.alpha = 0f;
    }

    public void SetInfo(string text, float duration, EToastColor color, EToastPosition position)
    {
        Show(text, duration, colors[(int)color], position);
    }

    private void Show(string text, float duration, Color color, EToastPosition position)
    {
        _uiText.text = (text.Length > maxTextLength) ? text.Substring(0, maxTextLength) + "..." : text;
        _uiImage.color = color;
        _container.childAlignment = (TextAnchor)((int)position);

        Dismiss();
        StartCoroutine(CoFadeInOut(duration, fadeDuration));
    }

    private IEnumerator CoFadeInOut(float toastDuration, float fadeDuration)
    {
        yield return null;
        _container.CalculateLayoutInputHorizontal();
        _container.CalculateLayoutInputVertical();
        _container.SetLayoutHorizontal();
        _container.SetLayoutVertical();
        yield return null;
        // Anim start
        yield return Fade(_uiCanvasGroup, 0f, 1f, fadeDuration);
        yield return new WaitForSeconds(toastDuration);
        yield return Fade(_uiCanvasGroup, 1f, 0f, fadeDuration);
        // Anim end
    }

    private IEnumerator Fade(CanvasGroup cGroup, float startAlpha, float endAlpha, float fadeDuration)
    {
        float startTime = Time.time;
        float alpha = startAlpha;

        if (fadeDuration > 0f)
        {
            //Anim start
            while (alpha != endAlpha)
            {
                alpha = Mathf.Lerp(startAlpha, endAlpha, (Time.time - startTime) / fadeDuration);
                cGroup.alpha = alpha;

                yield return null;
            }
        }

        cGroup.alpha = endAlpha;
    }

    public void Dismiss()
    {
        StopAllCoroutines();
        _uiCanvasGroup.alpha = 0f;
    }

    private void OnDestroy()
    {
        Managers.UI.isToastLoaded = false;
    }
}
