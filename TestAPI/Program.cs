using TestAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "GRPCPolicy", builder =>
    {
        builder.WithOrigins("http://localhost:5008","https://localhost:7250")
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true }); 
// Configure the HTTP request pipeline.
app.UseCors("GRPCPolicy");

app.MapGrpcService<GreeterService>();
app.MapGrpcService<PeopleService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
