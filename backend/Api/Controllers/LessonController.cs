using Application.Interfaces;
using Application.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/lessons")]
    [Authorize]
    public class LessonController : ApiControllerBase
    {
        private readonly ILessonServices _lessonServices;

        public LessonController(ILessonServices lessonServices)
        {
            _lessonServices = lessonServices;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Secretary")]
        public async Task<IActionResult> Create([FromBody] LessonRequestDTO request)
        {
            var response = await _lessonServices.CreateAsync(request);
            return FromStatus(response.Status, response);
        }

        [HttpPost("recurring")]
        [Authorize(Roles = "Admin,Secretary")]
        public async Task<IActionResult> CreateRecurring(
            [FromBody] CreateRecurringLessonDTO request)
        {
            var response = await _lessonServices.CreateRecurringAsync(request);
            return FromStatus(response.Status, response);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Secretary,Teacher")]
        public async Task<IActionResult> GetByDateRange(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] Guid? teacherId,
            [FromQuery] Guid? studentId)
        {
            var response = await _lessonServices.GetByDateRangeAsync(
                from,
                to,
                teacherId,
                studentId,
                TeacherScopeEmail);

            return FromStatus(response.Status, response);
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Admin,Secretary,Teacher")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var response = await _lessonServices.GetByIdAsync(id, TeacherScopeEmail);
            return FromStatus(response.Status, response);
        }

        [HttpPatch("{id:guid}/cancel")]
        [Authorize(Roles = "Admin,Secretary")]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] CancelLessonRequestDTO request)
        {
            var response = await _lessonServices.CancelAsync(id, request);
            return FromStatus(response.Status, response);
        }

        [HttpPatch("{id:guid}/complete")]
        [Authorize(Roles = "Admin,Secretary")]
        public async Task<IActionResult> Complete(Guid id)
        {
            var response = await _lessonServices.CompleteAsync(id);
            return FromStatus(response.Status, response);
        }

        [HttpPatch("{id:guid}/reschedule")]
        [Authorize(Roles = "Admin,Secretary")]
        public async Task<IActionResult> Reschedule(
            Guid id,
            [FromBody] RescheduleLessonRequestDTO request)
        {
            var response = await _lessonServices.RescheduleAsync(id, request);
            return FromStatus(response.Status, response);
        }

        [HttpPatch("series/{seriesId:guid}/cancel")]
        [Authorize(Roles = "Admin,Secretary")]
        public async Task<IActionResult> CancelSeries(
            Guid seriesId,
            [FromBody] CancelLessonSeriesRequestDTO request)
        {
            var response = await _lessonServices.CancelSeriesAsync(seriesId, request);
            return FromStatus(response.Status, response);
        }
    }
}
