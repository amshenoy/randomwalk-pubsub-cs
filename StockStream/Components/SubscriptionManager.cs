using Grpc.Core;
using System.Collections.Concurrent;

namespace StockStream.Components;

public class SubscriptionManager<Category, Event, Response> where Category : notnull
{
    // Using ConcurrentDictionary with IServerStreamWriter as key and byte as value (acting like a HashSet)
    private readonly ConcurrentDictionary<Category, ConcurrentDictionary<IServerStreamWriter<Response>, byte>> _observers = new();

	private readonly Func<Event, Category> _eventCategoryMapper;
	private readonly Func<Event, Response> _eventResponseMapper;

	public SubscriptionManager(Func<Event, Category> eventCategoryMapper, Func<Event, Response> eventResponseMapper)
	{
		_eventCategoryMapper = eventCategoryMapper;
		_eventResponseMapper = eventResponseMapper;
	}

    public void Register(Category category, IServerStreamWriter<Response> observer)
    {
        var observers = _observers.GetOrAdd(category, _ => new());
        observers[observer] = 0;
    }

    public void Unregister(Category category, IServerStreamWriter<Response> observer)
	{
		if (!_observers.TryGetValue(category, out var observers)) return;
		observers.TryRemove(observer, out _);
		if (!observers.IsEmpty) return;
		_observers.TryRemove(category, out _);
	}

	public void Broadcast(Event eventObj)
    {
		var category = _eventCategoryMapper(eventObj);
        if (_observers.TryGetValue(category, out var observers))
        {
            var response = _eventResponseMapper(eventObj);
            foreach (var observer in observers.Keys)
            {
                try
                {
					_ = Task.Run(() => observer.WriteAsync(response));
                }
                catch (Exception) {} // Connection closed
            }
        }
    }
}
