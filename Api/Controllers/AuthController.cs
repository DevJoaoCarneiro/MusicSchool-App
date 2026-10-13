using Application.Request;
using Application.Interfaces;
using Application.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


namespace Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly IAuthServices _authServices;

        public AuthController(IAuthServices authServices)
        {
            _authServices = authServices;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                var bad = new AuthResponseDTO
                {
                    Message = "Invalid request parameters",
                    Status = "InvalidArgument",
                    Data = null
                };

                return BadRequest(bad);
            }

            var response = await _authServices.LoginAsync(request);

            if (response.Status == "InvalidArgument")
            {
                return BadRequest(response);
            }

            if (response.Status == "Unauthorized")
            {
                return Unauthorized(response);
            }

            if (response.Status == "Error")
            {
                return StatusCode(500, response);
            }

            return Ok(response);
        }
    }
}