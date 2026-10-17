
using System;

namespace Application.Response
{
    public class EnrollmentResponseDTO
    {
        public string Message { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public EnrollmentData? Data { get; set; }
    }

    public class EnrollmentData
    {
        public Guid Id { get; set; }

        public StudentEnrollmentData Student { get; set; } = new();

        public InstrumentEnrollmentData Instrument { get; set; } = new();

        public DateTime StartDate { get; set; }

        public decimal MonthlyFee { get; set; }

        public int DueDay { get; set; }

        public string Status { get; set; } = string.Empty;
    }

    public class StudentEnrollmentData
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    public class InstrumentEnrollmentData
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}