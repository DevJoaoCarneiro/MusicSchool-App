using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using Application.Interfaces;
using Application.Response;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class TuitionService : ITuitionServices
    {
        private readonly ITuitionRepository _tuitionRepository;

        public TuitionService(ITuitionRepository tuitionRepository)
        {
            _tuitionRepository = tuitionRepository;
        }

        public async Task<TuitionListResponseDTO> GetAllAsync(
            string? status,
            string? referenceMonth,
            Guid? studentId,
            int page,
            int pageSize)
        {
            if (page < 1 || pageSize < 1)
            {
                return Error(
                    "Page and pageSize must be greater than zero",
                    "invalid_argument");
            }

            TuitionStatus? tuitionStatus = null;

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (string.Equals(
                    status, "Pending",
                    StringComparison.OrdinalIgnoreCase))
                {
                    tuitionStatus = TuitionStatus.Pending;
                }
                else if (string.Equals(
                    status, "Paid",
                    StringComparison.OrdinalIgnoreCase))
                {
                    tuitionStatus = TuitionStatus.Paid;
                }
                else
                {
                    return Error(
                        "Status must be Pending or Paid",
                        "invalid_argument");
                }
            }

            if (!string.IsNullOrWhiteSpace(referenceMonth) &&
                !DateTime.TryParseExact(
                    referenceMonth,
                    "yyyy-MM",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out _))
            {
                return Error(
                    "referenceMonth must use the yyyy-MM format",
                    "invalid_argument");
            }

            if (studentId == Guid.Empty)
            {
                return Error(
                    "studentId must be a valid identifier",
                    "invalid_argument");
            }

            try
            {
                var result = await _tuitionRepository.GetAllAsync(
                    tuitionStatus,
                    referenceMonth,
                    studentId,
                    page,
                    pageSize);

                var today = DateTime.Today;

                return new TuitionListResponseDTO
                {
                    Message = "Tuitions retrieved successfully",
                    Status = "Success",
                    Data = new TuitionListData
                    {
                        Items = result.Items.Select(t => new TuitionListItemDTO
                        {
                            Id = t.Id,
                            StudentName = t.Enrollment.Student.Name,
                            Instrument = t.Enrollment.Instrument.Name,
                            ReferenceMonth = t.ReferenceMonth,
                            DueDate = t.DueDate,
                            Amount = t.Amount,
                            Status = t.Status.ToString(),
                            IsOverdue =
                                t.Status == TuitionStatus.Pending &&
                                t.DueDate.Date < today
                        }).ToList(),
                        Page = page,
                        PageSize = pageSize,
                        Total = result.Total
                    }
                };
            }
            catch (Exception)
            {
                return Error(
                    "An unexpected error occurred while retrieving tuitions",
                    "error");
            }
        }

        private static TuitionListResponseDTO Error(
            string message,
            string status)
        {
            return new TuitionListResponseDTO
            {
                Message = message,
                Status = status,
                Data = null
            };
        }
    }
}