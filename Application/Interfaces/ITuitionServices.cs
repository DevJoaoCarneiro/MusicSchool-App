using System;
using System.Collections.Generic;
using System.Text;
using Application.Response;

namespace Application.Interfaces
{
    public interface ITuitionServices
    {
        Task<TuitionListResponseDTO> GetAllAsync(
            string? status,
            string? referenceMonth,
            Guid? studentId,
            int page,
            int pageSize);
    }
}