using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

public sealed class StockRepository
{
    private static readonly Lazy<StockRepository> _instance = new(() => new StockRepository());
    public static StockRepository Instance => _instance.Value;

    private readonly ConcurrentDictionary<string, Stock> _stocks = new();

    private StockRepository()
    {
        var random = new Random();
        AddStock(new Stock("GOOGL", random.Next(50, 200)));
        AddStock(new Stock("AMZN", random.Next(50, 200)));
        AddStock(new Stock("MSFT", random.Next(50, 200)));
        AddStock(new Stock("AAPL", random.Next(50, 200)));
        AddStock(new Stock("NFLX", random.Next(50, 200)));
    }

    public void AddStock(Stock stock)
    {
        _stocks[stock.Symbol] = stock;
    }

    public IReadOnlyCollection<Stock> GetStocks()
    {
        return _stocks.Values.ToList().AsReadOnly();
    }

    public Stock GetStock(string symbol)
    {
        return _stocks.TryGetValue(symbol, out var stock) ? stock : null;
    }
}

public record StockPriceChangedEvent(string Symbol, double Price);

public class Stock
{
    private readonly string _symbol;
    private double _price;

    public Stock(string symbol, double price)
    {
        _symbol = symbol;
        _price = price;
    }

    public string Symbol => _symbol;
    public double Price => _price;

    public void UpdatePrice(double price)
    {
        _price = price;
        DomainEvents.Publish(new StockPriceChangedEvent(_symbol, _price));
    }
}

public class StockPriceChangedEventListener
{
    public void HandleEvent(StockPriceChangedEvent eventObj) {
        Console.WriteLine(eventObj);
        StockPriceChangedSubject.Instance.Notify(eventObj);
    }
}

public class RandomStockPriceUpdatingTask
{
    private readonly StockRepository _repository = StockRepository.Instance;

    public async Task RunAsync()
    {
        while (true)
        {
            UpdateRandomStock();
            await Task.Delay(100); // Non-blocking delay
        }
    }

    private void UpdateRandomStock()
    {
        var random = new Random();
        var stocks = _repository.GetStocks();
        var randomStock = stocks.Skip(random.Next(stocks.Count)).FirstOrDefault();
        if (randomStock != null)
        {
            var newPrice = randomStock.Price + random.NextDouble() * 10 - 5;
            randomStock.UpdatePrice(newPrice);
        }
    }
}


