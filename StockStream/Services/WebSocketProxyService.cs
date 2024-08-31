using Grpc.Net.Client;
using Microsoft.AspNetCore.Http;
using StockGrpc;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

public class WebSocketProxyService
{
    private readonly ConcurrentDictionary<string, WebSocket> _clients = new();
    private readonly StockService.StockServiceClient _grpcClient;

    public WebSocketProxyService(StockService.StockServiceClient grpcClient)
    {
        _grpcClient = grpcClient;
    }

    public async Task HandleWebSocketAsync(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var clientId = context.Connection.Id;
        _clients.TryAdd(clientId, webSocket);

        try
        {
            await ProcessWebSocketAsync(clientId, webSocket);
        }
        finally
        {
            _clients.TryRemove(clientId, out _);
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        }
    }

    internal class RequestDto
	{
		public string? Type { get; set; }
		public string[]? Symbols { get; set; }
	}

	internal JsonSerializerOptions JsonSerializerOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		PropertyNameCaseInsensitive = true, // Optional: Allows case-insensitive property matching
		Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true) }
	};

    private async Task ProcessWebSocketAsync(string clientId, WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        var cancellationToken = CancellationToken.None;

        // Establish gRPC stream using HTTP/2
        using var call = _grpcClient.PriceStream();
        var responseStream = call.ResponseStream;

        // Task to read from gRPC stream and send to WebSocket
        var grpcStreamTask = Task.Run(async () =>
        {
            try
            {
                while (await responseStream.MoveNext(cancellationToken))
                {
                    var stockResponse = responseStream.Current;
					// Console.WriteLine(stockResponse.ToString());
                    var responseMessage = JsonSerializer.Serialize(stockResponse); // TODO: Use a different deserializer options?
                    var responseBuffer = Encoding.UTF8.GetBytes(responseMessage);
                    await webSocket.SendAsync(
						new ArraySegment<byte>(responseBuffer),
						WebSocketMessageType.Text,
						endOfMessage: true,
						cancellationToken
					);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in gRPC stream task: {ex.Message}");
            }
        }, cancellationToken);

        // Process incoming WebSocket messages
        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                // Close gRPC stream if WebSocket is closed
                await call.RequestStream.CompleteAsync();
                break;
            }

            if (result.MessageType == WebSocketMessageType.Text)
            {
                var requestMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
				Console.WriteLine("Raw WS Message {0}", requestMessage);

				// This is unable to allocate to a readonly array of strings
                // var req = JsonSerializer.Deserialize<StockStreamRequest>(requestMessage, JsonSerializerOptions);

                var requestJson = JsonSerializer.Deserialize<RequestDto>(requestMessage, JsonSerializerOptions);
				var type = Enum.TryParse<RequestType>(requestJson?.Type, ignoreCase: true, out var parsedType) ? parsedType : RequestType.Subscribe;
        		var symbolsList = requestJson?.Symbols ?? [];
				var req = new StockStreamRequest{ Type = type };
				req.Symbols.AddRange(symbolsList);

				// Console.WriteLine($"Stock Stream Request: {requestJson?.Type} {req}");
				Console.WriteLine($"Stock Stream Request: {req}");
                if (req != null)
                {
                    await call.RequestStream.WriteAsync(req);
                }
            }
        }

        // Ensure gRPC stream is completed
        await call.RequestStream.CompleteAsync();
        await grpcStreamTask;
    }
}
