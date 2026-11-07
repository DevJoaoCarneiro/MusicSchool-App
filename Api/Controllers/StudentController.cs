using Application.Interfaces;
using Application.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/students")]
    [Route("api/Student")]
    [Authorize(Roles = "Admin,Secretary,Teacher")]
    public class StudentController : ApiControllerBase
    {
        private readonly IStudentServices _studentService;

        public StudentController(IStudentServices studentService)
        {
            _studentService = studentService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Secretary")]
        public async Task<IActionResult> Create(
            [FromBody] StudentRequestDTO studentRequestDTO)
        {
            var result = await _studentService.CreateStudent(studentRequestDTO);
            return FromStatus(result.Status, result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Secretary")]
        public async Task<IActionResult> GetById(string id)
        {
            if (!Guid.TryParse(id, out var studentId))
                return InvalidId("student");

            var result = await _studentService.GetByIdAsync(studentId);
            return FromStatus(result.Status, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? name,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _studentService.GetAllAsync(name, page, pageSize);
            return FromStatus(result.Status, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Secretary")]
        public async Task<IActionResult> Update(
            string id,
            [FromBody] StudentRequestDTO studentRequestDTO)
        {
            if (!Guid.TryParse(id, out var studentId))
                return InvalidId("student");

            var result = await _studentService.UpdateStudentAsync(studentId, studentRequestDTO);
            return FromStatus(result.Status, result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Secretary")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!Guid.TryParse(id, out var studentId))
                return InvalidId("student");

            var result = await _studentService.DeleteStudentAsync(studentId);
            return FromStatus(result.Status, result);
        }
    }
}
