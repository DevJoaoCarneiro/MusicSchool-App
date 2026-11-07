using Application.Interfaces;
using Application.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/teachers")]
    [Authorize(Roles = "Admin,Secretary,Teacher")]
    public class TeacherController : ApiControllerBase
    {
        private readonly ITeacherServices _teacherService;

        public TeacherController(ITeacherServices teacherService)
        {
            _teacherService = teacherService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Secretary")]
        public async Task<IActionResult> Register(
            [FromBody] TeacherRequestDTO teacherRequestDTO)
        {
            var result = await _teacherService.CreateTeacher(teacherRequestDTO);
            return FromStatus(result.Status, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetTeachers(
            [FromQuery] string? name,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _teacherService.GetTeachers(name, page, pageSize);
            return FromStatus(result.Status, result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Secretary")]
        public async Task<IActionResult> GetTeacherById(string id)
        {
            if (!Guid.TryParse(id, out var teacherId))
                return InvalidId("teacher");

            var result = await _teacherService.GetTeacherById(teacherId);
            return FromStatus(result.Status, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Secretary")]
        public async Task<IActionResult> UpdateTeacher(
            string id,
            [FromBody] TeacherRequestDTO teacherRequestDTO)
        {
            if (!Guid.TryParse(id, out var teacherId))
                return InvalidId("teacher");

            var result = await _teacherService.UpdateTeacher(teacherId, teacherRequestDTO);
            return FromStatus(result.Status, result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Secretary")]
        public async Task<IActionResult> DeleteTeacher(string id)
        {
            if (!Guid.TryParse(id, out var teacherId))
                return InvalidId("teacher");

            var result = await _teacherService.DeleteTeacher(teacherId);
            return FromStatus(result.Status, result);
        }
    }
}
