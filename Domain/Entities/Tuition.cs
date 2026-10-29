using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class Tuition
    {
        public Guid Id { get; private set; }

        public Guid EnrollmentId { get; private set; }

        public Enrollment Enrollment { get; private set; } = null!;

        public string ReferenceMonth { get; private set; } = string.Empty;

        public DateTime DueDate { get; private set; }

        public decimal Amount { get; private set; }

        public TuitionStatus Status { get; private set; }

        public Tuition()
        {
        }
    }
}