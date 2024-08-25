using Grpc.Core;
using StockGrpc;
using System.Threading.Tasks;

namespace StockGrpc.Services;

public class StockService : StockGrpc.StockService.StockServiceBase
{
    private readonly StockRepository _repository;
    private readonly StockPriceChangedSubject _subject;

    // Constructor injection for dependencies
    public StockService(StockRepository repository, StockPriceChangedSubject subject)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _subject = subject ?? throw new ArgumentNullException(nameof(subject));
    }
    
    public override Task<StockPriceResponse> GetPrice(StockPriceRequest request, ServerCallContext context)
    {
        var stock = _repository.GetStock(request.Symbol) ?? throw new RpcException(new Status(StatusCode.NotFound, "Stock not found"));

        var response = new StockPriceResponse
        {
            Symbol = stock.Symbol,
            Price = stock.Price
        };
        
        return Task.FromResult(response);
    }

    public override async Task SubscribePrice(StockPriceRequest request, IServerStreamWriter<StockPriceResponse> responseStream, ServerCallContext context)
    {
        var stock = _repository.GetStock(request.Symbol) ?? throw new RpcException(new Status(StatusCode.NotFound, "Stock not found"));
        
        var response = new StockPriceResponse
        {
            Symbol = stock.Symbol,
            Price = stock.Price
        };
        await responseStream.WriteAsync(response);

        // Register the observer for future price updates
        _subject.Register(request.Symbol, responseStream);

        try
        {
            // Keep the stream open to continue receiving updates
            while (!context.CancellationToken.IsCancellationRequested)
            {
                await Task.Delay(Timeout.Infinite, context.CancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
            // Client disconnected or canceled the stream
        }
        finally
        {
            // Unregister the stream to clean up resources
            _subject.Unregister(request.Symbol, responseStream);
        }

        // // Handle stream cancellation (client disconnects)
        // context.CancellationToken.Register(() =>
        // {
        //     _subject.Unregister(request.Symbol, responseStream);
        // });
    }
}
