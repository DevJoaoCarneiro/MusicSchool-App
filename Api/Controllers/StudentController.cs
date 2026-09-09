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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                if (!Guid.TryParse(id, out Guid studentId))
                {
                    return BadRequest(new
                    {
                        Message = "Invalid student ID format",
                        Status = "invalid_argument",
                        Data = (object?)null
                    });
                }

                var result = await _studentService.GetByIdAsync(studentId);

                return result.Status switch
                {
                    "not_found" => NotFound(result),

                    "error" => StatusCode(
                        StatusCodes.Status500InternalServerError,
                        result
                    ),

                    _ => Ok(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting student by ID");

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        Message = "An unexpected error occurred",
                        Status = "error",
                        Data = (object?)null
                    }
                );
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? name,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _studentService.GetAllAsync(
                    name,
                    page,
                    pageSize
                );

                return result.Status switch
                {
                    "invalid_argument" => BadRequest(result),

                    "error" => StatusCode(
                        StatusCodes.Status500InternalServerError,
                        result
                    ),

                    _ => Ok(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving students");

                return StatusCode(
                    StatusCodes.Status500InternalServerError
                );
            }
        }

        // ALTERAR ALUNO
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            string id,
            [FromBody] StudentRequestDTO studentRequestDTO)
        {
            try
            {
                // Validar formato del ID
                if (!Guid.TryParse(id, out Guid studentId))
                {
                    return BadRequest(new
                    {
                        Message = "Invalid student ID format",
                        Status = "invalid_argument",
                        Data = (object?)null
                    });
                }

                var result = await _studentService.UpdateStudentAsync(
                    studentId,
                    studentRequestDTO
                );

                return result.Status switch
                {
                    "invalid_argument" => BadRequest(result),

                    "not_found" => NotFound(result),

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
                _logger.LogError(ex, "Unexpected error while updating student");

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        Message = "An unexpected error occurred",
                        Status = "error",
                        Data = (object?)null
                    }
                );
            }
        }
    

    [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                if (!Guid.TryParse(id, out Guid studentId))
                {
                    return BadRequest(new
                    {
                        Message = "Invalid student ID format",
                        Status = "invalid_argument",
                        Data = (object?)null
                    });
                }

                var result = await _studentService.DeleteStudentAsync(studentId);

                return result.Status switch
                {
                    "not_found" => NotFound(result),

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
                _logger.LogError(ex, "Unexpected error while deleting student");

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        Message = "An unexpected error occurred",
                        Status = "error",
                        Data = (object?)null
                    }
                );
            }
        }

    }

}