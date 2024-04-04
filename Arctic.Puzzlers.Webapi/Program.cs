using Arctic.Puzzlers.Parsers;
using Arctic.Puzzlers.Stores;
using Arctic.Puzzlers.Webapi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddParser();
builder.Services.AddDataStores(builder.Configuration);
builder.Services.AddHostedService<Worker>();
var app = builder.Build();

app.UseSwagger();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{    
    app.UseSwaggerUI();
    
}
app.UseHttpsRedirection();

app.MapControllers();

app.Run();
