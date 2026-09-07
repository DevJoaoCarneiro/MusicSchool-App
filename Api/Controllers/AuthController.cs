using Application.Request;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


namespace Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
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
            var response = await _authServices.LoginAsync(request);

            if (response.Status == "invalid_argument")
            {
                return BadRequest(response);
            }

            if (response.Status == "Unauthorized")
            {
                return Unauthorized(response);
            }

            if (response.Status == "error")
            {
                return StatusCode(500, response);
            }

            return Ok(response);
        }
    }
}