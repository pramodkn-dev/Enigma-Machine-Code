using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PriceHub.Service.Services;
using PriceHub.Service.Store;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGrpc().AddJsonTranscoding();
builder.Services.AddGrpcReflection();
builder.Services.AddSingleton<OracleDb>();
builder.Services.AddSingleton<ListPriceRepository>();
builder.Services.AddSingleton<PriceJobRepository>();
var app = builder.Build();
app.MapGrpcService<PriceServiceImpl>();
app.MapGrpcReflectionService();
app.MapGet("/", () => "PriceHub (.NET 8) — gRPC + REST");
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.Run();
