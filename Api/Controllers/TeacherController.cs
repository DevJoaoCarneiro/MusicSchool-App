using Application.Interfaces;
using Application.Request;
using Application.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers
{
    /// <summary>
    /// Endpoints for managing teachers.
    /// </summary>
    [ApiController]
    [Route("api/teachers")]
    [Authorize]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherServices _teacherService;
        private readonly ILogger<TeacherController> _logger;

        public TeacherController(
            ITeacherServices teacherService,
            ILogger<TeacherController> logger)
        {
            _teacherService = teacherService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Secretary")]

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register(
            [FromBody] TeacherRequestDTO teacherRequestDTO)
        {
            try
            {
                var result =
                    await _teacherService.CreateTeacher(teacherRequestDTO);

                return result.Status switch
                {
                    "invalid_argument" => BadRequest(result),
                    "not_found" => NotFound(result),
                    "conflict" => Conflict(result),
                    "error" => StatusCode(
                        StatusCodes.Status500InternalServerError,
                        result),
                    _ => Ok(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while registering teacher");

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        Message = "An unexpected error occurred",
                        Status = "error",
                        Data = (object?)null
                    });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTeachers(
                   [FromQuery] string? name,
                   [FromQuery] int page = 1,
                   [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _teacherService.GetTeachers(
                    name,
                    page,
                    pageSize);

                return result.Status switch
                {
                    "invalid_argument" => BadRequest(result),
                    "error" => StatusCode(
                        StatusCodes.Status500InternalServerError,
                        result),
                    _ => Ok(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while getting teachers");

                return StatusCode(
                    StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTeacherById(string id)
        {
            if (!Guid.TryParse(id, out var teacherId))
            {
                return BadRequest(new TeacherResponseDTO
                {
                    Message = "Invalid teacher ID",
                    Status = "invalid_argument",
                    Data = null
                });
            }

            try
            {
                var result = await _teacherService.GetTeacherById(teacherId);

                return result.Status switch
                {
                    "invalid_argument" => BadRequest(result),
                    "not_found" => NotFound(result),
                    "error" => StatusCode(
                        StatusCodes.Status500InternalServerError, result),
                    _ => Ok(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving teacher");

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new TeacherResponseDTO
                    {
                        Message = "An unexpected error occurred",
                        Status = "error",
                        Data = null
                    });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Secretary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateTeacher(
            string id,
            [FromBody] TeacherRequestDTO teacherRequestDTO)
        {
            if (!Guid.TryParse(id, out var teacherId))
            {
                return BadRequest(new TeacherResponseDTO
                {
                    Message = "Invalid teacher ID",
                    Status = "invalid_argument",
                    Data = null
                });
            }

            try
            {
                var result = await _teacherService.UpdateTeacher(
                    teacherId, teacherRequestDTO);

                return result.Status switch
                {
                    "invalid_argument" => BadRequest(result),
                    "not_found" => NotFound(result),
                    "conflict" => Conflict(result),
                    "error" => StatusCode(500, result),
                    _ => Ok(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating teacher");

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new TeacherResponseDTO
                    {
                        Message = "An unexpected error occurred",
                        Status = "error",
                        Data = null
                    });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Secretary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteTeacher(string id)
        {
            if (!Guid.TryParse(id, out var teacherId))
            {
                return BadRequest(new TeacherResponseDTO
                {
                    Message = "Invalid teacher ID",
                    Status = "invalid_argument",
                    Data = null
                });
            }

            try
            {
                var result = await _teacherService.DeleteTeacher(teacherId);

                return result.Status switch
                {
                    "invalid_argument" => BadRequest(result),
                    "not_found" => NotFound(result),
                    "conflict" => Conflict(result),
                    "error" => StatusCode(
                        StatusCodes.Status500InternalServerError, result),
                    _ => Ok(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting teacher");

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new TeacherResponseDTO
                    {
                        Message = "An unexpected error occurred",
                        Status = "error",
                        Data = null
                    });
            }
        }
    }
}