
public class RandomStockPriceUpdatingService : BackgroundService
{
    private readonly StockRepository _repository;
    private readonly Random _random = new();
	public int UpdateIntervalMilliseconds = 40;

    public RandomStockPriceUpdatingService(StockRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            UpdateRandomStock();
            await Task.Delay(UpdateIntervalMilliseconds, stoppingToken);
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


