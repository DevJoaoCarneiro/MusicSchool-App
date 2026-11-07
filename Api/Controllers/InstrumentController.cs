using Application.Interfaces;
using Application.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/instruments")]
    [Authorize]
    public class InstrumentController : ApiControllerBase
    {
        private readonly IInstrumentServices _services;

        public InstrumentController(IInstrumentServices services)
        {
            _services = services;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(
            [FromBody] InstrumentRequestDTO request)
        {
            var response = await _services.CreateAsync(request);
            return FromStatus(response.Status, response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _services.GetAllAsync();
            return FromStatus(response.Status, response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            if (!Guid.TryParse(id, out var instrumentId))
                return InvalidId("instrument");

            var response = await _services.GetByIdAsync(instrumentId);
            return FromStatus(response.Status, response);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(
            string id,
            [FromBody] InstrumentRequestDTO request)
        {
            if (!Guid.TryParse(id, out var instrumentId))
                return InvalidId("instrument");

            var response = await _services.UpdateAsync(instrumentId, request);
            return FromStatus(response.Status, response);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!Guid.TryParse(id, out var instrumentId))
                return InvalidId("instrument");

            var response = await _services.DeleteAsync(instrumentId);
            return FromStatus(response.Status, response);
        }
    }
}
