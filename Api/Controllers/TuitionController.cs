using Application.Interfaces;
using Application.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/tuitions")]
    [Authorize(Roles = "Admin,Secretary")]
    public class TuitionController : ApiControllerBase
    {
        private readonly ITuitionService _tuitionService;

        public TuitionController(ITuitionService tuitionService)
        {
            _tuitionService = tuitionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? referenceMonth,
            [FromQuery] string? status,
            [FromQuery] Guid? enrollmentId,
            [FromQuery] Guid? studentId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var response = await _tuitionService.GetAllAsync(
                referenceMonth,
                status,
                enrollmentId,
                studentId,
                page,
                pageSize);

            return FromStatus(response.Status, response);
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate(
            [FromBody] GenerateTuitionsRequestDTO request)
        {
            var response = await _tuitionService.GenerateAsync(request);
            return FromStatus(response.Status, response);
        }

        [HttpPatch("{id}/payment")]
        public async Task<IActionResult> RegisterPayment(
            string id,
            [FromBody] RegisterTuitionPaymentRequestDTO request)
        {
            if (!Guid.TryParse(id, out var tuitionId))
                return InvalidId("tuition");

            var response = await _tuitionService.RegisterPaymentAsync(tuitionId, request);
            return FromStatus(response.Status, response);
        }
    }
}
