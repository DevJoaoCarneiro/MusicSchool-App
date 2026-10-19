using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class Enrollment
    {
        public Guid Id { get; private set; }

        public Guid StudentId { get; private set; }
        public Student Student { get; private set; } = null!;

        public Guid InstrumentId { get; private set; }
        public Instrument Instrument { get; private set; } = null!;

        public DateTime StartDate { get; private set; }

        public decimal MonthlyFee { get; private set; }

        public int DueDay { get; private set; }

        public EnrollmentStatus Status { get; private set; }

        // Constructor para Entity Framework Core
        public Enrollment()
        {
        }

        // Constructor para crear una matrícula
        public Enrollment(
            Guid studentId,
            Guid instrumentId,
            DateTime startDate,
            decimal monthlyFee,
            int dueDay)
        {
            Id = Guid.NewGuid();
            StudentId = studentId;
            InstrumentId = instrumentId;
            StartDate = startDate;
            MonthlyFee = monthlyFee;
            DueDay = dueDay;
            Status = EnrollmentStatus.Active;
        }

        public void Deactivate()
        {
            Status = EnrollmentStatus.Inactive;
        }
    }

}
