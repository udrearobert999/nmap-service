using Vantage.Application.Worker;
using Vantage.Infrastructure.Persistence;
using Vantage.Infrastructure.Worker;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);
var configuration = builder.Configuration;

builder.Services
    .AddApplication(configuration)
    .AddPersistence(configuration)
    .AddInfrastructure(configuration);

builder.Services.AddSerilog((services, loggerConfiguration) => {
    loggerConfiguration.ReadFrom.Configuration(builder.Configuration);
});

var host = builder.Build();
host.Run();