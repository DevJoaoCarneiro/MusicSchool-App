using Application.Interfaces;
using Application.Request;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentServices _studentService;
        private readonly ILogger<StudentController> _logger;

        public StudentController(
            IStudentServices studentService,
            ILogger<StudentController> logger)
        {
            _studentService = studentService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] StudentRequestDTO studentRequestDTO)
        {
            try
            {
                var result = await _studentService.CreateStudent(studentRequestDTO);

                return result.Status switch
                {
                    "invalid_argument" => BadRequest(result),
                    "conflict" => Conflict(result),
                    "error" => StatusCode(
                        StatusCodes.Status500InternalServerError,
                        result
                    ),
                    _ => Ok(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating student");

                return StatusCode(
                    StatusCodes.Status500InternalServerError
                );
            }
        }
    }
}