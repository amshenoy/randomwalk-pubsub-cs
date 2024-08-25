using Grpc.Core;
using StockGrpc;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace StockGrpc.Services;

public class StockService : StockGrpc.StockService.StockServiceBase
{
    private readonly StockRepository _repository;
    private readonly StockPriceChangedSubject _subject;

    private readonly ConcurrentDictionary<string, HashSet<string>> clientActiveSymbols = new();


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

    // public override async Task SubscribePrice(StockPriceRequest request, IServerStreamWriter<StockPriceResponse> responseStream, ServerCallContext context)
    // {
    //     var stock = _repository.GetStock(request.Symbol) ?? throw new RpcException(new Status(StatusCode.NotFound, "Stock not found"));
        
    //     var response = new StockPriceResponse
    //     {
    //         Symbol = stock.Symbol,
    //         Price = stock.Price
    //     };
    //     await responseStream.WriteAsync(response);

    //     // Register the observer for future price updates
    //     _subject.Register(request.Symbol, responseStream);

    //     try
    //     {
    //         // Keep the stream open to continue receiving updates
    //         while (!context.CancellationToken.IsCancellationRequested)
    //         {
    //             await Task.Delay(Timeout.Infinite, context.CancellationToken);
    //         }
    //     }
    //     catch (TaskCanceledException)
    //     {
    //         // Client disconnected or canceled the stream
    //     }
    //     finally
    //     {
    //         // Unregister the stream to clean up resources
    //         _subject.Unregister(request.Symbol, responseStream);
    //     }

    //     // // Handle stream cancellation (client disconnects)
    //     // context.CancellationToken.Register(() =>
    //     // {
    //     //     _subject.Unregister(request.Symbol, responseStream);
    //     // });
    // }

    // This method handles the bi-directional stream
    public override async Task PriceStream(IAsyncStreamReader<StockStreamRequest> requestStream, IServerStreamWriter<StockPriceResponse> responseStream, ServerCallContext context)
    {
        // Create a unique identifier for this client connection
        var clientId = context.GetHttpContext().Connection.Id;
        var activeSymbols = new HashSet<string>();

        // Register the client with an empty set of active symbols
        clientActiveSymbols[clientId] = activeSymbols;

        // Task for handling incoming requests
        var requestTask = Task.Run(async () =>
        {
            await foreach (var request in requestStream.ReadAllAsync())
            {
                switch (request.Type)
                {
                    case RequestType.Subscribe:
                        Console.WriteLine($"Client {clientId} subscribed to stock: {request.Symbol}");
                        lock (activeSymbols)
                        {
                            activeSymbols.Add(request.Symbol);
                        }
                        break;

                    case RequestType.Unsubscribe:
                        Console.WriteLine($"Client {clientId} unsubscribed from stock: {request.Symbol}");
                        lock (activeSymbols)
                        {
                            activeSymbols.Remove(request.Symbol);
                        }
                        break;
                }
            }

            // Clean up when the client disconnects
            clientActiveSymbols.TryRemove(clientId, out _);
        });

        // Task for sending responses
        var responseTask = Task.Run(async () =>
        {
            var random = new Random();
            while (!context.CancellationToken.IsCancellationRequested)
            {
                List<string> symbolsToUpdate;

                lock (activeSymbols)
                {
                    symbolsToUpdate = new List<string>(activeSymbols);
                }

                foreach (var symbol in symbolsToUpdate)
                {
                    var price = random.NextDouble() * 1000;
                    var response = new StockPriceResponse
                    {
                        Symbol = symbol,
                        Price = price
                    };

                    await responseStream.WriteAsync(response);
                    Console.WriteLine($"Sent price update for {symbol} to client {clientId}: {price}");
                }

                await Task.Delay(1000); // Delay between updates
            }
        });

        await Task.WhenAll(requestTask, responseTask);
    }

}
