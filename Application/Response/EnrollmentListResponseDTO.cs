
using System;
using System.Collections.Generic;

namespace Application.Response
{
    public class EnrollmentListResponseDTO
    {
        public string Message { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public EnrollmentListData? Data { get; set; }
    }

    public class EnrollmentListData
    {
        public List<EnrollmentListItemDTO> Items { get; set; } = new();

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int Total { get; set; }
    }

    public class EnrollmentListItemDTO
    {
        public Guid Id { get; set; }

        public string StudentName { get; set; } = string.Empty;

        public string InstrumentName { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
