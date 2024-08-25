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
	// }

	// This method handles the bi-directional stream
	public override async Task PriceStream(
		IAsyncStreamReader<StockStreamRequest> requestStream,
		IServerStreamWriter<StockPriceResponse> responseStream,
		ServerCallContext context
	)
	{
		// Create a unique identifier for this client connection
		var clientId = context.GetHttpContext().Connection.Id;
		var activeSymbols = new HashSet<string>();

		// Register the client with an empty set of active symbols
		clientActiveSymbols[clientId] = activeSymbols;

		// Task for handling incoming requests
		var token = context.CancellationToken;
		var requestTask = Task.Run(async () =>
		{
			try
			{
				while (!token.IsCancellationRequested)
                {
					// ISSUE: ReadAllAsync does not promptly react to cancellation
					// hence throwing exception instead of safely exiting the loop :-(
					// https://github.com/dotnet/runtime/issues/56820
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
								_subject.Register(request.Symbol, responseStream);
								break;

							case RequestType.Unsubscribe:
								Console.WriteLine($"Client {clientId} unsubscribed from stock: {request.Symbol}");
								lock (activeSymbols)
								{
									activeSymbols.Remove(request.Symbol);
								}
								_subject.Unregister(request.Symbol, responseStream);
								break;
						}
					}
				}
			}
			catch (Exception ex)
			{
				// Request Stream is aborted
				Console.WriteLine($"Request Stream Closed: {ex.Message}");
			}
		}, token);

		try
		{
			// Keep the stream open to continue receiving updates
			await requestTask;
		}
		catch (TaskCanceledException)
		{
			// Client disconnected or canceled the stream
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Unhandled Exception: {ex.Message}");
		}
		finally
		{
			// Unregister the stream to clean up resources
			lock (activeSymbols)
			{
				foreach (var symbol in activeSymbols)
				{
					_subject.Unregister(symbol, responseStream);
				}
			}
			// Clean up when the client disconnects
			clientActiveSymbols.TryRemove(clientId, out _);
		}
	}

}
