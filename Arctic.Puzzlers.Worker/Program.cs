using Arctic.Puzzlers.Parsers;
using Arctic.Puzzlers.Worker;
using Arctic.Puzzlers.Stores;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddParser();
builder.Services.AddDataStores();
var host = builder.Build();
host.Run();
