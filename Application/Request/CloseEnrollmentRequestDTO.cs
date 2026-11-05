using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Request
{
    public class CloseEnrollmentRequestDTO
    {
        public DateOnly? EndDate { get; set; }

        public string? Reason { get; set; }
    }
}
