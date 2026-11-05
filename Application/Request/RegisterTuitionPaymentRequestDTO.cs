namespace Application.Request
{
    public class RegisterTuitionPaymentRequestDTO
    {
        public DateOnly? PaidAt { get; set; }

        public decimal? Amount { get; set; }

        public string? Method { get; set; }
    }
}