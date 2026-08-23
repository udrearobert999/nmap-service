using Microsoft.AspNetCore.Mvc;
using NetworkMapper.Domain.Results;
using NetworkMapper.Domain.Results.Abstractions;

namespace NetworkMapper.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApiControllerBase : ControllerBase
{
    protected IActionResult HandleFailure(Result result)
    {
        if (result.IsSuccess && result.HasError())
            throw new InvalidOperationException("Result marked as successful but it contains error");

        if (result.IsValidationFailure())
            return BadRequest(
                CreateProblemDetails(
                    StatusCodes.Status400BadRequest,
                    result.Error,
                    result.Error.InnerErrors
                ));

        if (result.IsNotFound())
            return NotFound();

        return BadRequest(
            CreateProblemDetails(
                StatusCodes.Status400BadRequest,
                result.Error
            ));
    }

    private static ProblemDetails CreateProblemDetails(
        int status,
        IError error,
        IEnumerable<IError>? errors = null) =>
        new()
        {
            Type = error.Code,
            Title = "One or more validation errors occurred.",
            Detail = error.Message,
            Status = status,
            Extensions = { { nameof(errors), errors } }
        };
}