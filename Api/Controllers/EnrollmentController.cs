using Application.Interfaces;
using Application.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/enrollments")]
    [Authorize(Roles = "Admin")]
    public class EnrollmentController : ControllerBase
    {
        private readonly IEnrollmentServices _enrollmentServices;

        public EnrollmentController(
            IEnrollmentServices enrollmentServices)
        {
            _enrollmentServices = enrollmentServices;
        }

        [HttpPost]
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
    }

}
