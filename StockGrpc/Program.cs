using StockGrpc.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<StockService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");


var eventListener = new StockPriceChangedEventListener();
DomainEvents.Subscribe<StockPriceChangedEvent>(eventListener.HandleEvent);

// TODO: Use a background task
var updateTask = new RandomStockPriceUpdatingTask();
Console.WriteLine("Price Updater Started");
updateTask.RunAsync();

Console.WriteLine("Starting App");

app.Run();



