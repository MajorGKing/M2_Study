using DG.Tweening;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;

public class DamageFont : MonoBehaviour
{
    private TextMeshPro _damageText;

    private void Awake()
    {
        GetComponent<MeshRenderer>().sortingOrder = SortingLayers.DAMAGE_FONT;
    }

    public void SetInfo(float damage = 0, Transform parent = null, EFontType fontType = EFontType.Hit)
    {
        _damageText = GetComponent<TextMeshPro>();
        _damageText.sortingOrder = SortingLayers.PROJECTILE;
        transform.position = parent.transform.position;
        transform.localScale = Vector3.zero;

        switch (fontType)
        {
            case EFontType.Hit:
                _damageText.fontSize = 6;
                _damageText.color = Color.white; //Util.HexToColor("EFAD00");
                _damageText.text = $"{Mathf.Abs((int)damage)}";
                DoAnimation();
                break;
            case EFontType.Critical:
                _damageText.fontSize = 6;
                _damageText.text = $"{Mathf.Abs((int)damage)}";
                _damageText.color = Utils.HexToColor("E94141");
                DoAnimation();
                break;
            case EFontType.Miss:
                _damageText.fontSize = 7;
                _damageText.text = "Miss";
                _damageText.color = Color.red;
                DoAnimation();
                break;
            case EFontType.Heal:
                _damageText.fontSize = 6;
                _damageText.text = $"{Mathf.Abs((int)damage)}";
                _damageText.color = Utils.HexToColor("3DA55A");
                DoAnimation();
                break;
            case EFontType.Cost:
                _damageText.fontSize = 6;
                _damageText.text = $"{((int)damage)}";
                _damageText.color = Color.blue;
                DoAnimation();
                break;
            case EFontType.Stun:
                _damageText.fontSize = 7;
                _damageText.text = "TODO 기절";
                _damageText.color = Color.cyan;
                DoAnimation();
                break;
            case EFontType.Gold:
                _damageText.fontSize = 6;
                _damageText.text = $"Gold +{(int)damage}";
                _damageText.color = Color.yellow;
                DoAnimation();
                break;
            case EFontType.Exp:
                _damageText.fontSize = 6;
                _damageText.text = $"Exp +{(int)damage}";
                _damageText.color = Color.green;
                DoAnimation();
                break;
        }

        _damageText.alpha = 1;

    }

    private void DoAnimation()
    {
        Sequence seq = DOTween.Sequence();

        transform.localScale = Vector3.zero;
        transform.position += Vector3.up;

        seq.Append(transform.DOScale(1f, 0.3f).SetEase(Ease.Linear))
            .Join(transform.DOMove(transform.position + Vector3.up * 1.5f, 1f).SetEase(Ease.Linear))
            // .Append(transform.DOScale(1.0f, 1.5f).SetEase(Ease.Linear))
            // .Join(transform.GetComponent<TMP_Text>().DOFade(0, 0.5f).SetEase(Ease.InQuint))
            .OnComplete(() => { Managers.Resource.Destroy(gameObject); });
    }

    //private void DoAnimation()
    //{
    //    Sequence seq = DOTween.Sequence();

    //    //1. 크기가 0~ 110퍼 까지 커졌다가 100퍼까지 돌아간다
    //    //2. 서서히 사라진다
    //    transform.localScale = Vector3.one;

    //    seq.Append(transform.DOScale(1.3f, 0.3f).SetEase(Ease.InOutBounce))
    //         .Join(transform.DOMove(transform.position + Vector3.up * 2f, 0.5f).SetEase(Ease.OutCirc))
    //         //.Append(transform.DOScale(1.0f, 0.3f).SetEase(Ease.OutBounce))
    //         //.Join(transform.GetComponent<TMP_Text>().DOFade(0, 0.3f).SetEase(Ease.InQuint))
    //         .Append(GetComponent<TextMeshPro>().DOFade(0, 0.3f).SetEase(Ease.Linear))
    //         .OnComplete(() =>
    //         {
    //             Managers.Resource.Destroy(gameObject);
    //         });

    //}

    //void NormalDamageFont()
    //{
    //    Sequence seq = DOTween.Sequence();

    //    //1. 크기가 0~ 110퍼 까지 커졌다가 100퍼까지 돌아간다
    //    //2. 서서히 사라진다
    //    transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
    //    transform.position += Vector3.up * Random.Range(1.5f, 2f);
    //    float randomX = Random.Range(-1.5f, 1.5f);
    //    Vector3 movePos = new Vector3(randomX, -1.5f);
    //    Vector3 rotation = Random.Range(-1f, 1f) > 0f ? Vector3.forward * -20f : Vector3.forward * 20f;

    //    seq.Append(transform.DOScale(0.5f, 0.7f).SetEase(Ease.OutSine))
    //    .Join(transform.DOMoveX(transform.position.x + randomX, 0.7f).SetEase(Ease.OutSine))
    //    .Join(transform.DOMoveY(transform.position.y - 2.5f, 0.7f).SetEase(Ease.InSine))
    //    .Join(transform.DORotate(rotation, 0.7f))

    //    .OnComplete(() =>
    //    {
    //        Managers.Resource.Destroy(gameObject);
    //    });
    //}

    //void CriticalDamageFont()
    //{
    //    Sequence seq = DOTween.Sequence();

    //    transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
    //    transform.position += Vector3.up * Random.Range(2f, 2.5f);
    //    float randomX = Random.Range(-1.5f, 1.5f);
    //    Vector3 movePos = new Vector3(randomX, -1.5f);
    //    Vector3 rotation = Random.Range(-1f, 1f) > 0f ? Vector3.forward * -20f : Vector3.forward * 20f;


    //    seq.Append(transform.DOScale(0.0f, 0.7f).SetEase(Ease.InBack))
    //    .Join(transform.DOMove(transform.position + movePos, 0.7f).SetEase(Ease.InBack))
    //    .Join(transform.DORotate(rotation, 0.7f))

    //    .OnComplete(() =>
    //    {
    //        Managers.Resource.Destroy(gameObject);
    //    });
    //}
}
