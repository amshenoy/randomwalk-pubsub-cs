using StockGrpc;
using StockGrpc.Components;
using StockGrpc.Services;

using StockPriceSubscriptionManager = StockGrpc.Components.SubscriptionManager<string, StockPriceChangedEvent, StockGrpc.StockPriceResponse>;

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

var app = builder.Build();

// app.UseMiddleware<GrpcErrorHandlerMiddleware>();
app.MapGrpcService<StockGrpc.Services.StockService>();
app.MapGet("/", () => """
Communication with gRPC endpoints must be made through a gRPC client.
To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909
""");

var eventListener = app.Services.GetRequiredService<StockPriceSubscriptionManager>();
DomainEvents.Subscribe<StockPriceChangedEvent>(eventListener.Broadcast);

Console.WriteLine("Starting App");
app.Run();
