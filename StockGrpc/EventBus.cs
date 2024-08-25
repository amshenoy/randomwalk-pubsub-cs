using System;
using System.Collections.Generic;

// TODO: Make this threadsafe
public class EventBus
{
    private readonly Dictionary<Type, List<Action<object>>> _handlers = [];

    public void Register<T>(Action<T> handler)
    {
        var eventType = typeof(T);
        if (!_handlers.ContainsKey(eventType))
        {
            _handlers[eventType] = [];
        }

        _handlers[eventType].Add(x => handler((T)x));
    }

    public void Post<T>(T eventObj)
    {
        var eventType = eventObj.GetType();
        if (_handlers.TryGetValue(eventType, out List<Action<object>>? handlers))
        {
            foreach (var handler in handlers)
            {
                handler(eventObj);
            }
        }
    }
}

public static class DomainEvents
{
    private static readonly EventBus eventBus = new();
    static DomainEvents() {}
    public static void Publish<T>(T eventObj) => eventBus.Post(eventObj);
    public static void Subscribe<T>(Action<T> eventListener) => eventBus.Register(eventListener);
}

