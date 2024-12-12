using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_SubItem : UI_Base
{
    [SerializeField]
    protected ScrollRect _parentScrollRect;

    protected override void Awake()
    {
        base.Awake();

        _parentScrollRect = Utils.FindAncestor<ScrollRect>(gameObject);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _parentScrollRect.OnBeginDrag(eventData);
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        _parentScrollRect.OnDrag(eventData);
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        _parentScrollRect.OnEndDrag(eventData);
    }
}
