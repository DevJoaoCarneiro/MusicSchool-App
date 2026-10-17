using System;
using System.Collections.Generic;
using System.Text;
using Application.Request;
using Application.Response;

namespace Application.Interfaces
{
    public interface IEnrollmentServices
    {
        Task<EnrollmentResponseDTO> CreateEnrollmentAsync(
            EnrollmentRequestDTO request);
    }
}
