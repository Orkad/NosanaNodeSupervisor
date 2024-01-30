using Docker.DotNet;
using NosanaNodeSupervisor;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSerilog();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<DockerClientConfiguration>();
builder.Services.AddSingleton<INosanaNode, DockerNosanaNode>();

var host = builder.Build();
host.Run();
