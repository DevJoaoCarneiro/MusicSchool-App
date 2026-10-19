
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/lessons")]
    [Authorize(Roles = "Admin")]
    public class LessonController : ControllerBase
    {
        private readonly ILessonServices _lessonServices;

        public LessonController(ILessonServices lessonServices)
        {
            _lessonServices = lessonServices;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] LessonRequestDTO request)
        {
            var response = await _lessonServices.CreateAsync(request);

            return response.Status switch
            {
                "Success" => Ok(response),
                "BadRequest" => BadRequest(response),
                "NotFound" => NotFound(response),
                "Conflict" => Conflict(response),
                _ => StatusCode(
                    StatusCodes.Status500InternalServerError,
                    response)
            };
        }
    }
}
