using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;
using static UnityEditor.Searcher.Searcher.AnalyticsEvent;

public class EventManager
{
    private Dictionary<EEventType, Action> _events = new Dictionary<EEventType, Action>();

    public void AddEvent(EEventType eventType, Action listener)
    {
        if (_events.ContainsKey(eventType) == false)
            _events.Add(eventType, new Action(() => { }));

        _events[eventType] += listener;
    }

    public void RemoveEvent(EEventType eventType, Action listener)
    {
        if(_events.ContainsKey(eventType))
            _events[eventType] -= listener;
    }

    public void TriggerEvent(EEventType eventType)
    {
        if(_events.ContainsKey(eventType))
            _events[eventType].Invoke();
    }

    public void Clear()
    {
        _events.Clear();
    }
}
