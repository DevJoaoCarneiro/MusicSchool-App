
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/lessons")]
    public class LessonController : ControllerBase
    {
        private readonly ILessonServices _lessonServices;

        public LessonController(ILessonServices lessonServices)
        {
            _lessonServices = lessonServices;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
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

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetByDateRange(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] Guid? teacherId,
            [FromQuery] Guid? studentId)
        {
            try
            {
                var response = await _lessonServices.GetByDateRangeAsync(
                    from,
                    to,
                    teacherId,
                    studentId);

                return response.Status switch
                {
                    "Success" => Ok(response),
                    "BadRequest" => BadRequest(response),
                    _ => StatusCode(
                        StatusCodes.Status500InternalServerError,
                        response)
                };
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        Message = "An unexpected error occurred",
                        Status = "Error",
                        Data = (object?)null
                    });
            }
        }
    }
}
