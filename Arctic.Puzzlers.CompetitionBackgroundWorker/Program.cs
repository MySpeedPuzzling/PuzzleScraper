using Arctic.Puzzlers.CompetitionBackgroundWorker;
using Arctic.Puzzlers.Parsers.CompetitionParsers.Options;
using Arctic.Puzzlers.Parsers;
using Arctic.Puzzlers.Stores;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.ConfigureOptions<CompetitionParsingOptionsSetup>();
builder.Services.AddParser();
builder.Services.AddDataStores(builder.Configuration);
var host = builder.Build();
host.Run();
