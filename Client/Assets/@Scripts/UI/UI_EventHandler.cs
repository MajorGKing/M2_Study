using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_EventHandler : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Action<PointerEventData> OnClickHandler = null;
    public Action<PointerEventData> OnPointerDownHandler = null;
    public Action<PointerEventData> OnPointerUpHandler = null;
    public Action<PointerEventData> OnDragHandler = null;
    public Action<PointerEventData> OnBeginDragHandler = null;
    public Action<PointerEventData> OnEndDragHandler = null;

    private bool _isDragging = false;
    private PointerEventData _currentEventData;

    private void Update()
    {
        if (_isDragging)
            OnDragHandler?.Invoke(_currentEventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
		OnClickHandler?.Invoke(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnPointerDownHandler?.Invoke(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnPointerUpHandler?.Invoke(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        //드래그중 포인터를 가만히 있으면 이벤트가 발생하지 않기때문에 Update에서 따로 이벤트 발생시킴
        _currentEventData = eventData;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _currentEventData = eventData;
        OnBeginDragHandler?.Invoke(eventData);
        _isDragging = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;
        OnEndDragHandler?.Invoke(eventData);
    }
}
