using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Serilog;
using Vantage.Application;
using Vantage.Domain.Abstractions;
using Vantage.Infrastructure;
using Vantage.Infrastructure.Persistence;
using Vantage.WebAPI.Caching.Extensions;
using Vantage.WebAPI.Config;
using Vantage.WebAPI.Extensions;
using Vantage.WebAPI.Options;
using Vantage.WebAPI.Security;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

configuration.AddEnvironmentVariables();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddAuthentication(DevSubjectAuthenticationHandler.SchemeName)
        .AddScheme<AuthenticationSchemeOptions, DevSubjectAuthenticationHandler>(
            DevSubjectAuthenticationHandler.SchemeName, _ => { });
}
else
{
    builder.Services.AddOptions<ClerkOptions>()
        .Bind(configuration.GetSection(ClerkOptions.SectionName));

    builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
        .Configure<IOptions<ClerkOptions>>((jwtOptions, clerk) =>
        {
            jwtOptions.Authority = clerk.Value.Authority;
            jwtOptions.TokenValidationParameters.ValidateAudience = !string.IsNullOrWhiteSpace(clerk.Value.Audience);
            jwtOptions.TokenValidationParameters.ValidAudience = clerk.Value.Audience;
        });

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer();
}

builder.Services.AddAuthorization();

builder.Services
    .AddApplication()
    .AddPersistence(configuration)
    .AddInfrastructure(configuration);

builder.Services.AddScoped<CurrentTeamAccessor>();
builder.Services.AddScoped<ICurrentTeamAccessor>(sp => sp.GetRequiredService<CurrentTeamAccessor>());
builder.Services.AddScoped<CurrentUserAccessor>();
builder.Services.AddScoped<ICurrentUserAccessor>(sp => sp.GetRequiredService<CurrentUserAccessor>());

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
app.UseIdentityResolution();

app.MapControllers();

app.ApplyMigrations();

app.Run();