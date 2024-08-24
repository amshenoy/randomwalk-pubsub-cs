using System;
using System.Collections.Generic;

public class EventBus
{
    private readonly Dictionary<Type, List<Action<object>>> _handlers = new Dictionary<Type, List<Action<object>>>();

    public void Register<T>(Action<T> handler)
    {
        var eventType = typeof(T);
        if (!_handlers.ContainsKey(eventType))
        {
            _handlers[eventType] = new List<Action<object>>();
        }

        _handlers[eventType].Add(x => handler((T)x));
    }

    public void Post<T>(T eventObj)
    {
        var eventType = eventObj.GetType();
        if (_handlers.ContainsKey(eventType))
        {
            foreach (var handler in _handlers[eventType])
            {
                handler(eventObj);
            }
        }
    }
}

public static class DomainEvents
{
    private static readonly EventBus eventBus = new EventBus();
    static DomainEvents() {}
    public static void Publish<T>(T eventObj) => eventBus.Post(eventObj);
    public static void Subscribe<T>(Action<T> eventListener) => eventBus.Register(eventListener);
}

