using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Serilog;
using NetworkMapper.Application;
using NetworkMapper.Infrastructure;
using NetworkMapper.Infrastructure.Persistence;
using NetworkMapper.WebAPI.Caching.Extensions;
using NetworkMapper.WebAPI.Config;
using NetworkMapper.WebAPI.Extensions;
using NetworkMapper.WebAPI.Options;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

configuration.AddEnvironmentVariables();

builder.Services.AddOptions<Auth0Options>()
    .Bind(configuration.GetSection(Auth0Options.SectionName));

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<Auth0Options>>((jwtOptions, auth0) =>
    {
        jwtOptions.Authority = auth0.Value.Domain;
        jwtOptions.Audience = auth0.Value.Audience;
    });

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();
builder.Services.AddAuthorization();

builder.Services
    .AddApplication()
    .AddPersistence(configuration)
    .AddInfrastructure(configuration);

builder.Services
    .AddControllers(options =>
    {
        options.Conventions.Add(new RouteTokenTransformerConvention(new SpinalCaseRouteNameTransformer()));
    });

builder.Services.AddOutputCache(options => { options.ConfigureCustomPolicies(); });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter your JWT token directly below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document), []
        }
    });
});
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.ApplyMigrations();

app.Run();