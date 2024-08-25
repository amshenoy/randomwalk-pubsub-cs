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

	public override async Task PriceStream(
		IAsyncStreamReader<StockStreamRequest> requestStream,
		IServerStreamWriter<StockPriceResponse> responseStream,
		ServerCallContext context
	)
	{
		var clientId = context.GetHttpContext().Connection.Id;
		clientActiveSymbols[clientId] = [];

		Console.WriteLine($"Client Connected: {clientId}");

		var token = context.CancellationToken;
		try
		{
			var requestTask = Task.Run(() => StockStreamListener(clientId, requestStream, responseStream, token), token);
			await requestTask;
		}
		catch (TaskCanceledException)
		{
			// Client disconnected or canceled the stream
			Console.WriteLine($"Client Disconnected: {clientId}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Unhandled Exception: {ex.Message}");
		}
		finally
		{
			if (clientActiveSymbols.TryGetValue(clientId, out var activeSymbols)) {
				lock (activeSymbols)
				{
					foreach (var symbol in activeSymbols)
					{
						// Unsubscribe from symbols for current client
						_subject.Unregister(symbol, responseStream);
					}
				}
				// Clean up when the client disconnects
				clientActiveSymbols.TryRemove(clientId, out _);
			}
		}
	}

	private async Task StockStreamListener(
		string clientId,
		IAsyncStreamReader<StockStreamRequest> requestStream,
		IServerStreamWriter<StockPriceResponse> responseStream,
		CancellationToken token
	)
	{
		try {
			var activeSymbols = clientActiveSymbols[clientId];
			// ISSUE: ReadAllAsync does not promptly react to cancellation
			// hence throwing exception instead of safely exiting the loop :-(
			// https://github.com/dotnet/runtime/issues/56820
			await foreach (var request in requestStream.ReadAllAsync(token))
			{
				switch (request.Type)
				{
					case RequestType.Subscribe:
						Console.WriteLine($"Client {clientId} subscribed to symbols: {string.Join(", ", request.Symbols)}");
						lock (activeSymbols)
						{
							activeSymbols.UnionWith(request.Symbols);
						}
						foreach (var symbol in request.Symbols) {
							_subject.Register(symbol, responseStream);
						}
						break;

					case RequestType.Unsubscribe:
						Console.WriteLine($"Client {clientId} unsubscribed from symbols: {string.Join(", ", request.Symbols)}");
						lock (activeSymbols)
						{
							activeSymbols.ExceptWith(request.Symbols);
						}
						foreach (var symbol in request.Symbols) {
							_subject.Unregister(symbol, responseStream);
						}
						break;
				}
			}
		}
		catch (Exception) {
			Console.WriteLine($"Client Disconnected: {clientId}");
		}
	}

}
