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

var authOptions = configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>() ?? new AuthOptions();
var authMode = authOptions.Resolve(builder.Environment.IsDevelopment());

if (authMode == AuthMode.DevHeaders)
{
    if (!builder.Environment.IsDevelopment())
        throw new InvalidOperationException(
            "Auth:Mode=DevHeaders trusts unauthenticated X-Dev-* headers and is only allowed in Development.");

    builder.Services.AddAuthentication(DevSubjectAuthenticationHandler.SchemeName)
        .AddScheme<AuthenticationSchemeOptions, DevSubjectAuthenticationHandler>(
            DevSubjectAuthenticationHandler.SchemeName, _ => { });
}
else
{
    builder.Services.AddOptions<ClerkOptions>()
        .Bind(configuration.GetSection(ClerkOptions.SectionName))
        .Validate(clerk => !string.IsNullOrWhiteSpace(clerk.Authority),
            "Clerk:Authority is required when Auth:Mode=Clerk.")
        .ValidateOnStart();

    builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
        .Configure<IOptions<ClerkOptions>>((jwtOptions, clerk) =>
        {
            jwtOptions.Authority = clerk.Value.Authority;
            jwtOptions.MapInboundClaims = false;
            jwtOptions.TokenValidationParameters.NameClaimType = "sub";
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