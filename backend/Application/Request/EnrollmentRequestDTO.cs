using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Application.Request
{
    public class EnrollmentRequestDTO
    {
        [Required]
        public Guid StudentId { get; set; }

        [Required]
        public Guid InstrumentId { get; set; }

        [Required]
        public DateTime? StartDate { get; set; }

        [Required]
        [Range(0.01, 9999999999)]
        public decimal MonthlyFee { get; set; }

        [Required]
        [Range(1, 28)]
        public int DueDay { get; set; }
    }
}
