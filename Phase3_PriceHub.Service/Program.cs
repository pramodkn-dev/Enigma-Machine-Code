using PriceHub.Service.Services;

var builder = WebApplication.CreateBuilder(args);

// CORS (demo-friendly; tighten for production)
builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(p => p
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// gRPC with JSON transcoding
builder.Services.AddGrpc().AddJsonTranscoding();

// Reflection (for grpcurl and service discovery)
builder.Services.AddGrpcReflection();

var app = builder.Build();

app.UseCors();

// Map gRPC service
app.MapGrpcService<PriceServiceImpl>();

// Enable reflection in Development
if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

// Health and info endpoints
app.MapGet("/", () => "PriceHub.Service is running. Use gRPC or JSON-transcoded HTTP routes.");
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
