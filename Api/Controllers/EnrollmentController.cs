
using System;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/enrollments")]
    public class EnrollmentController : ControllerBase
    {
        private readonly IEnrollmentServices _enrollmentServices;

        public EnrollmentController(
            IEnrollmentServices enrollmentServices)
        {
            _enrollmentServices = enrollmentServices;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateEnrollment(
            [FromBody] EnrollmentRequestDTO request)
        {
            var response =
                await _enrollmentServices.CreateEnrollmentAsync(request);

            return response.Status switch
            {
                "Success" => Ok(response),
                "invalid_argument" => BadRequest(response),
                "not_found" => NotFound(response),
                "conflict" => Conflict(response),
                _ => StatusCode(500, response)
            };
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
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
                page,
                pageSize);

            return response.Status switch
            {
                "Success" => Ok(response),
                "invalid_argument" => BadRequest(response),
                _ => StatusCode(500, response)
            };
        }
    }
}
