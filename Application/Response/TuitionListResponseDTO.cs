using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Response
{
    public class TuitionListResponseDTO
    {
        public string Message { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public TuitionListData? Data { get; set; }
    }

    public class TuitionListData
    {
        public List<TuitionListItemDTO> Items { get; set; } = new();

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int Total { get; set; }
    }

    public class TuitionListItemDTO
    {
        public Guid Id { get; set; }

        public string StudentName { get; set; } = string.Empty;

        public string Instrument { get; set; } = string.Empty;

        public string ReferenceMonth { get; set; } = string.Empty;

        public DateTime DueDate { get; set; }

        public decimal Amount { get; set; }

        public string Status { get; set; } = string.Empty;

        public bool IsOverdue { get; set; }
    }
}