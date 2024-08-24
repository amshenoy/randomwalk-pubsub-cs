using Grpc.Core;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using StockGrpc;

public class StockPriceChangedSubject
{
    private readonly ConcurrentDictionary<string, List<IServerStreamWriter<StockPriceResponse>>> _observers =
        new ConcurrentDictionary<string, List<IServerStreamWriter<StockPriceResponse>>>();

    // Constructor can be public as the DI container will manage the lifecycle
    public StockPriceChangedSubject()
    {
    }

    public void Register(string symbol, IServerStreamWriter<StockPriceResponse> observer)
    {
        _observers.AddOrUpdate(symbol, new List<IServerStreamWriter<StockPriceResponse>> { observer },
            (key, existingObservers) =>
            {
                existingObservers.Add(observer);
                return existingObservers;
            });
    }

    public void Unregister(string symbol, IServerStreamWriter<StockPriceResponse> observer)
    {
        if (_observers.TryGetValue(symbol, out var observers))
        {
            observers.Remove(observer);
            if (observers.Count == 0)
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
                await observer.WriteAsync(response);
            }
        }
    }
}
