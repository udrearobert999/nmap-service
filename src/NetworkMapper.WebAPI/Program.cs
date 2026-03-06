using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Serilog;
using NetworkMapper.Application;
using NetworkMapper.Infrastructure;
using NetworkMapper.Infrastructure.Persistence;
using NetworkMapper.WebAPI.Caching.Extensions;
using NetworkMapper.WebAPI.Config;
using NetworkMapper.WebAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

configuration.AddEnvironmentVariables();

builder.Services
    .AddApplication()
    .AddPersistence(configuration)
    .AddInfrastructure(configuration);

builder.Services
    .AddControllers(options =>
    {
        options.Conventions.Add(new RouteTokenTransformerConvention(new SpinalCaseRouteNameTransformer()));
    });

builder.Services.AddOutputCache(options =>
{
    options.ConfigureCustomPolicies();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Host.UseSerilog((_, config) => { config.ReadFrom.Configuration(configuration); });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseSerilogRequestLogging();
app.UseGlobalExceptionHandler();

app.UseCors();
app.UseRouting();

app.UseOutputCache();

app.UseAuthorization();

app.MapControllers();

app.ApplyMigrations();

app.Run();