using Application.Request;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthServices _authServices;

        public AuthController(IAuthServices authServices)
        {
            _authServices = authServices;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthRequestDTO request)
        {
            var response = await _authServices.LoginAsync(request);

            if (response.Status == "Unauthorized")
            {
                return Unauthorized(response);
            }

            return Ok(response);
        }
    }
}