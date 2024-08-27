using System.Collections.Concurrent;
using StockStream.Components;

namespace StockStream.Repositories;

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

public class Stock(string symbol, double price)
{
    private readonly string _symbol = symbol;
    private double _price = price;

	public string Symbol => _symbol;
    public double Price => _price;

    public void UpdatePrice(double price)
    {
        _price = price;
        DomainEvents.Publish(new StockPriceChangedEvent(_symbol, _price));
    }
}
