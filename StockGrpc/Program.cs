using StockGrpc;
using StockGrpc.Services;

using StockPriceSubscriptionManager = SubscriptionManager<string, StockPriceChangedEvent, StockGrpc.StockPriceResponse>;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<StockRepository>();
builder.Services.AddHostedService<RandomStockPriceUpdatingService>();
builder.Services.AddSingleton<StockPriceSubscriptionManager>(
	provider =>
    {
		static string eventCategoryMapper(StockPriceChangedEvent e) => e.Symbol;
		static StockPriceResponse eventResponseMapper(StockPriceChangedEvent e) => new(){ Symbol = e.Symbol, Price = e.Price };
		return new(eventCategoryMapper, eventResponseMapper);
    }
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
