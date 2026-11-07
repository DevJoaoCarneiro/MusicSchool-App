using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        protected const string TeacherRole = "Teacher";

        protected IActionResult FromStatus(string status, object response) => status switch
        {
            ResponseStatus.Success => Ok(response),
            ResponseStatus.InvalidArgument => BadRequest(response),
            ResponseStatus.Unauthorized => Unauthorized(response),
            ResponseStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, response),
            ResponseStatus.NotFound => NotFound(response),
            ResponseStatus.Conflict => Conflict(response),
            _ => StatusCode(StatusCodes.Status500InternalServerError, response)
        };

        protected IActionResult InvalidId(string entity) => BadRequest(new
        {
            Message = $"Invalid {entity} ID",
            Status = ResponseStatus.InvalidArgument,
            Data = (object?)null
        });

        protected string? CurrentEmail =>
            User.FindFirstValue(ClaimTypes.Email) ??
            User.FindFirstValue(JwtRegisteredClaimNames.Email);

        protected Guid CurrentUserId =>
            Guid.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue(JwtRegisteredClaimNames.Sub),
                out var id)
                ? id
                : Guid.Empty;

        protected string? TeacherScopeEmail =>
            User.IsInRole(TeacherRole) ? CurrentEmail ?? string.Empty : null;
    }
}
