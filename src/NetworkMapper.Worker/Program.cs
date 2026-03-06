using NetworkMapper.Worker;
using NetworkMapper.Application.Worker;
using NetworkMapper.Infrastructure.Persistence;
using NetworkMapper.Infrastructure.Worker;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);
var configuration = builder.Configuration;

builder.Services
    .AddApplication(configuration)
    .AddPersistence(configuration)
    .AddInfrastructure(configuration);

builder.Services.AddHostedService<Worker>();

builder.Services.AddSerilog((services, loggerConfiguration) => {
    loggerConfiguration.ReadFrom.Configuration(builder.Configuration);
});

var host = builder.Build();
host.Run();