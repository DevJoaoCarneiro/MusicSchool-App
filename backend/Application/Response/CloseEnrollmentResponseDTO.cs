using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Response
{
    public class CloseEnrollmentResponseDTO
    {
        public string Message { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public CloseEnrollmentData? Data { get; set; }
    }

    public class CloseEnrollmentData
    {
        public Guid Id { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateOnly EndDate { get; set; }
    }
}
