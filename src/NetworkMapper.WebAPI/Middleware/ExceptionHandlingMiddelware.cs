using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NetworkMapper.WebAPI.Middleware;

public class CustomProblemDetails : ProblemDetails
{
    public new IDictionary<string, object> Extensions { get; set; } = new Dictionary<string, object>();

    public bool ShouldSerializeExtensions()
    {
        return Extensions.Count > 0;
    }
}

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var endpointLogTitle = "[UnknownEndpoint]";
            var endpointName = context.GetEndpoint()?.DisplayName;
            if (!string.IsNullOrEmpty(endpointName))
                endpointLogTitle = $"[{endpointName}]";

            var problemDetails = new CustomProblemDetails
            {
                Title = "Internal Server Error",
                Status = context.Response.StatusCode,
                Detail = "Something went wrong!"
            };

            var jsonString = JsonConvert.SerializeObject(problemDetails, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            await context.Response.WriteAsync(jsonString);

            var logErrorMessage = ex.InnerException?.Message ?? ex.Message;
            _logger.LogError("{EndpointLogTitle} : {LogErrorMessage}", endpointLogTitle, logErrorMessage);
        }
    }
}