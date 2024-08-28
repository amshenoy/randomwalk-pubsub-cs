using System.Collections.Concurrent;

namespace StockStream.Components;

public class EventsHandler
{
	private readonly ConcurrentDictionary<Type, ConcurrentBag<Action<object>>> _handlers = new();

	public void Register<T>(Action<T> handler)
	{
		var eventType = typeof(T);
		var handlers = _handlers.GetOrAdd(eventType, _ => []);
		handlers.Add(x => handler((T)x));
	}

	public void Post<T>(T eventObj) where T : notnull
	{
		var eventType = eventObj.GetType();
		if (_handlers.TryGetValue(eventType, out var handlers))
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
	private static readonly EventsHandler eventsHandler = new();
	static DomainEvents() { }
	public static void Publish<T>(T eventObj) where T : notnull => eventsHandler.Post(eventObj);
	public static void Subscribe<T>(Action<T> eventListener) => eventsHandler.Register(eventListener);
}
