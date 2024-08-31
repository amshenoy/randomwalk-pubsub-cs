using Grpc.Net.Client;
using StockGrpc;
using StockStream.Components;
using StockStream.Repositories;
using StockStream.Services;

using StockPriceSubscriptionManager = StockStream.Components.SubscriptionManager<string, StockStream.Repositories.StockPriceChangedEvent, StockGrpc.StockPriceResponse>;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<StockRepository>();
builder.Services.AddHostedService<RandomStockPriceUpdatingService>();
builder.Services.AddSingleton<StockPriceSubscriptionManager>(
    _ => new(
        e => e.Symbol,
        e => new StockPriceResponse { Symbol = e.Symbol, Price = e.Price }
    )
);
builder.Services.AddGrpc();


// WebSocketProxyService needs to use a http client to access the same Grpc service
// However http clients have to have SSL enabled to use http2
// This also means that the app must now run with https!
// This makes it more difficult to setup a node grpc client as done previously
var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
};
var channel = GrpcChannel.ForAddress("https://localhost:5243", new GrpcChannelOptions
{
    HttpClient = new HttpClient(handler)
});
builder.Services.AddSingleton(sp => new StockGrpc.StockService.StockServiceClient(channel));


builder.Services.AddTransient<WebSocketProxyService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebSocketPolicy", policy =>
    {
        policy.WithOrigins("*")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowWebSocketPolicy");
app.UseRouting();
app.UseWebSockets();

// app.UseMiddleware<GrpcErrorHandlerMiddleware>();
app.MapGrpcService<StockStream.Services.StockService>();
app.MapGet("/", () => """
Communication with gRPC endpoints must be made through a gRPC client.
To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909
""");

app.Map("/ws", async context =>
{
	if (context.WebSockets.IsWebSocketRequest)
	{
		var webSocketProxyService = context.RequestServices.GetRequiredService<WebSocketProxyService>();
		await webSocketProxyService.HandleWebSocketAsync(context);
	}
	else
	{
		context.Response.StatusCode = StatusCodes.Status400BadRequest;
	}
});

var eventListener = app.Services.GetRequiredService<StockPriceSubscriptionManager>();
DomainEvents.Subscribe<StockPriceChangedEvent>(eventListener.Broadcast);

Console.WriteLine("Application running on port 5243");
app.Run("https://localhost:5243");
