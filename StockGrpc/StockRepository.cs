using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

public sealed class StockRepository
{
    private readonly ConcurrentDictionary<string, Stock> _stocks = new();

    public StockRepository()
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

    public Stock? GetStock(string symbol)
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
    private readonly StockPriceChangedSubject _subject;

    public StockPriceChangedEventListener(StockPriceChangedSubject subject)
    {
        _subject = subject ?? throw new ArgumentNullException(nameof(subject));
    }

    public void HandleEvent(StockPriceChangedEvent eventObj)
    {
        Console.WriteLine(eventObj);
        _subject.Notify(eventObj);  // Wait for the async method to complete
    }
}


public class RandomStockPriceUpdatingService : BackgroundService
{
    private readonly StockRepository _repository;
    private readonly Random _random = new();

    public RandomStockPriceUpdatingService(StockRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            UpdateRandomStock();
            await Task.Delay(100, stoppingToken);
        }
    }

    private void UpdateRandomStock()
    {
        var stocks = _repository.GetStocks();
        var randomStock = stocks.Skip(_random.Next(stocks.Count)).FirstOrDefault();
        if (randomStock is null) return;
        var newPrice = Math.Round(Math.Max(randomStock.Price + _random.NextDouble() * 10 - 5, 5), 2);
        randomStock.UpdatePrice(newPrice);
    }
}


