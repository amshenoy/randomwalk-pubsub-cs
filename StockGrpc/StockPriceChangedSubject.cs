using Grpc.Core;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using StockGrpc;

// TODO: Rename this to EventNotifier and use generics if needed
// Move the listener class to here

public class StockPriceChangedSubject
{
    // Changed List to ConcurrentBag for thread safety
    private readonly ConcurrentDictionary<string, ConcurrentBag<IServerStreamWriter<StockPriceResponse>>> _observers = new();

    public void Register(string symbol, IServerStreamWriter<StockPriceResponse> observer)
    {
        _observers.AddOrUpdate(symbol,
            _ => [observer],
            (_, existingObservers) =>
            {
                existingObservers.Add(observer);
                return existingObservers;
            });
    }

    public void Unregister(string symbol, IServerStreamWriter<StockPriceResponse> observer)
    {
        if (_observers.TryGetValue(symbol, out var observers))
        {
            // Remove the observer
            bool removed = observers.TryTake(out var existingObserver);
            while (!removed)
            {
                // In case observer is not found in the bag (unlikely but possible), retry
                removed = observers.TryTake(out existingObserver);
            }

            // Remove the symbol entry if there are no observers left
            if (observers.IsEmpty)
            {
                _observers.TryRemove(symbol, out _);
            }
        }
    }

    // Notify all observers about new event
    public async Task Notify(StockPriceChangedEvent eventObj)
    {
        if (_observers.TryGetValue(eventObj.Symbol, out var observers))
        {
            var response = new StockPriceResponse { Symbol = eventObj.Symbol, Price = eventObj.Price };
            foreach (var observer in observers)
            {
                try
                {
                    await observer.WriteAsync(response);
                }
                catch (Exception) {} // Connection closed
            }
        }
    }
}
