using Application.Interfaces;
using Application.Request;
using Application.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{

    [ApiController]
    [Route("api/tuitions")]
    public class TuitionController : ControllerBase
    {
        private readonly ITuitionService _tuitionService;
        private readonly ILogger<TuitionController> _logger;

        public TuitionController(
            ITuitionService tuitionService,
            ILogger<TuitionController> logger)
        {
            _tuitionService = tuitionService;
            _logger = logger;
        }

        [HttpPost("generate")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Generate(
            [FromBody] GenerateTuitionsRequestDTO request)
        {
            try
            {
                var response = await _tuitionService.GenerateAsync(request);

                return response.Status switch
                {
                    "Success" => Ok(response),
                    "invalid_argument" => BadRequest(response),
                    _ => StatusCode(
                        StatusCodes.Status500InternalServerError,
                        response)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while generating tuitions");

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new GenerateTuitionsResponseDTO
                    {
                        Message = "An unexpected error occurred",
                        Status = "error",
                        Data = null
                    });
            }
        }

        [HttpPatch("{id}/payment")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterPayment(
            string id,
            [FromBody] RegisterTuitionPaymentRequestDTO request)
        {
            try
            {
                if (!Guid.TryParse(id, out var tuitionId))
                {
                    return BadRequest(new RegisterTuitionPaymentResponseDTO
                    {
                        Message = "Invalid tuition ID",
                        Status = "invalid_argument",
                        Data = null
                    });
                }

                var response = await _tuitionService
                    .RegisterPaymentAsync(tuitionId, request);

                return response.Status switch
                {
                    "Success" => Ok(response),
                    "invalid_argument" => BadRequest(response),
                    "not_found" => NotFound(response),
                    "conflict" => Conflict(response),
                    _ => StatusCode(
                        StatusCodes.Status500InternalServerError,
                        response)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while registering tuition payment");

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new RegisterTuitionPaymentResponseDTO
                    {
                        Message = "An unexpected error occurred",
                        Status = "error",
                        Data = null
                    });
            }
        }
    }
}