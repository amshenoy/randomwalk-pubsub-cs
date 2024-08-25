using StockGrpc.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<StockRepository>();
builder.Services.AddHostedService<RandomStockPriceUpdatingService>();
builder.Services.AddSingleton<StockPriceChangedSubject>();
builder.Services.AddSingleton<StockPriceChangedEventListener>();

builder.Services.AddGrpc();

var app = builder.Build();

// app.UseMiddleware<GrpcErrorHandlerMiddleware>();
app.MapGrpcService<StockService>();
app.MapGet("/", () => """
Communication with gRPC endpoints must be made through a gRPC client.
To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909
""");

var eventListener = app.Services.GetRequiredService<StockPriceChangedEventListener>();
DomainEvents.Subscribe<StockPriceChangedEvent>(eventListener.HandleEvent);

Console.WriteLine("Starting App");
app.Run();
