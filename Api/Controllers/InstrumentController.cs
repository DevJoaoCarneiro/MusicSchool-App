
using Application.Interfaces;
using Application.Request;
using Application.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/instruments")]
    [Authorize]
    public class InstrumentController : ControllerBase
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

            if (response.Status == "Success" && response.Data is Application.Response.InstrumentData data)
            {
                return CreatedAtAction(nameof(GetById), new { id = data.Id }, response);
            }

            return MapResponse(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _services.GetAllAsync();
            return MapResponse(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            if (!Guid.TryParse(id, out var instrumentId))
            {
                return BadRequest(new InstrumentResponseDTO
                {
                    Message = "Invalid instrument ID",
                    Status = "BadRequest",
                    Data = null
                });
            }

            var response = await _services.GetByIdAsync(instrumentId);
            return MapResponse(response);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(
            string id,
            [FromBody] InstrumentRequestDTO request)
        {
            if (!Guid.TryParse(id, out var instrumentId))
            {
                return BadRequest(new InstrumentResponseDTO
                {
                    Message = "Invalid instrument ID",
                    Status = "BadRequest",
                    Data = null
                });
            }

            var response = await _services.UpdateAsync(instrumentId, request);
            return MapResponse(response);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!Guid.TryParse(id, out var instrumentId))
            {
                return BadRequest(new InstrumentResponseDTO
                {
                    Message = "Invalid instrument ID",
                    Status = "BadRequest",
                    Data = null
                });
            }

            var response = await _services.DeleteAsync(instrumentId);
            return MapResponse(response);
        }

        private IActionResult MapResponse(InstrumentResponseDTO response)
        {
            return response.Status switch
            {
                "Success" => Ok(response),
                "BadRequest" => BadRequest(response),
                "NotFound" => NotFound(response),
                "Conflict" => Conflict(response),
                _ => StatusCode(500, response)
            };
        }
    }
}
