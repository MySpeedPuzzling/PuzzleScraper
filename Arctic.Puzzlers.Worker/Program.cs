using Arctic.Puzzlers.Parsers;
using Arctic.Puzzlers.Worker;
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddParser();
var host = builder.Build();
host.Run();
