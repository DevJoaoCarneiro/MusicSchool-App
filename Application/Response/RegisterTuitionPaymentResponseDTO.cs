namespace Application.Response
{
    public class RegisterTuitionPaymentResponseDTO
    {
        public string Message { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public RegisterTuitionPaymentData? Data { get; set; }
    }

    public class RegisterTuitionPaymentData
    {
        public Guid Id { get; set; }

        public string ReferenceMonth { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateOnly PaidAt { get; set; }

        public string Method { get; set; } = string.Empty;
    }
}