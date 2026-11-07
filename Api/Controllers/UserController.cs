using Application.Interfaces;
using Application.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/users")]
    [Route("api/User")]
    [Authorize]
    public class UserController : ApiControllerBase
    {
        private readonly IUserServices _userService;

        public UserController(IUserServices userService)
        {
            _userService = userService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register([FromBody] UserRequestDTO userRequestDTO)
        {
            var result = await _userService.CreateUser(userRequestDTO);
            return FromStatus(result.Status, result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _userService.GetAllAsync(page, pageSize);
            return FromStatus(result.Status, result);
        }

        [HttpPatch("me/password")]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangePasswordRequestDTO request)
        {
            var result = await _userService.ChangePasswordAsync(CurrentUserId, request);
            return FromStatus(result.Status, result);
        }
    }
}
