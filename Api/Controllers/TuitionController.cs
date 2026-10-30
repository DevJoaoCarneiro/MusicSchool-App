using System;
using System.Threading.Tasks;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/tuitions")]
    [Authorize(Roles = "Admin")]
    public class TuitionController : ControllerBase
    {
        private readonly ITuitionServices _tuitionServices;

        public TuitionController(ITuitionServices tuitionServices)
        {
            _tuitionServices = tuitionServices;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? status,
            [FromQuery] string? referenceMonth,
            [FromQuery] Guid? studentId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var response = await _tuitionServices.GetAllAsync(
                status,
                referenceMonth,
                studentId,
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