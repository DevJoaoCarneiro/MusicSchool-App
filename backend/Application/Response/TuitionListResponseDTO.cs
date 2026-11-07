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

        public Guid EnrollmentId { get; set; }

        public string StudentName { get; set; } = string.Empty;

        public string InstrumentName { get; set; } = string.Empty;

        public string ReferenceMonth { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public DateOnly DueDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateOnly? PaidAt { get; set; }

        public decimal? PaidAmount { get; set; }

        public string? Method { get; set; }
    }
}
