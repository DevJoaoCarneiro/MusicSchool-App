using Application.Interfaces;
using Application.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/enrollments")]
    [Authorize]
    public class EnrollmentController : ApiControllerBase
    {
        private readonly IEnrollmentServices _enrollmentServices;

        public EnrollmentController(IEnrollmentServices enrollmentServices)
        {
            _enrollmentServices = enrollmentServices;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Secretary")]
        public async Task<IActionResult> CreateEnrollment(
            [FromBody] EnrollmentRequestDTO request)
        {
            var response = await _enrollmentServices.CreateEnrollmentAsync(request);
            return FromStatus(response.Status, response);
        }

        [HttpPatch("{id}/close")]
        [Authorize(Roles = "Admin,Secretary")]
        public async Task<IActionResult> CloseEnrollment(
            string id,
            [FromBody] CloseEnrollmentRequestDTO request)
        {
            if (!Guid.TryParse(id, out var enrollmentId))
                return InvalidId("matrícula");

            var response = await _enrollmentServices.CloseEnrollmentAsync(enrollmentId, request);
            return FromStatus(response.Status, response);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Secretary,Teacher")]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid? studentId,
            [FromQuery] Guid? instrumentId,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var response = await _enrollmentServices.GetAllAsync(
                studentId,
                instrumentId,
                status,
                TeacherScopeEmail,
                page,
                pageSize);

            return FromStatus(response.Status, response);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Secretary,Teacher")]
        public async Task<IActionResult> GetById(string id)
        {
            if (!Guid.TryParse(id, out var enrollmentId))
                return InvalidId("matrícula");

            var response = await _enrollmentServices.GetByIdAsync(enrollmentId, TeacherScopeEmail);
            return FromStatus(response.Status, response);
        }
    }
}
