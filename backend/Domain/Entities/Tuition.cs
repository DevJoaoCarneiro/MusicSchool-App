using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Domain.Entities
{
    public class Tuition
    {
        public const int ReferenceMonthLength = 7; // yyyy-MM

        public Guid Id { get; private set; }

        public Guid EnrollmentId { get; private set; }

        public Enrollment Enrollment { get; private set; } = null!;

        public string ReferenceMonth { get; private set; } = string.Empty;

        public decimal Amount { get; private set; }

        public DateTime DueDate { get; private set; }

        public TuitionStatus Status { get; private set; }

        public const decimal MaxAmount = 99_999_999.99m;

        public DateTime? PaidAt { get; private set; }

        public decimal? PaidAmount { get; private set; }

        public PaymentMethod? Method { get; private set; }

        public Tuition()
        {
        }

        public Tuition(Enrollment enrollment, string referenceMonth)
        {
            if (enrollment == null)
                throw new ArgumentNullException(nameof(enrollment));

            if (!TryParseReferenceMonth(referenceMonth, out var year, out var month))
                throw new ArgumentException(
                    "Reference month must be in the format yyyy-MM",
                    nameof(referenceMonth));

            Id = Guid.NewGuid();

            EnrollmentId = enrollment.Id;

            ReferenceMonth = FormatReferenceMonth(year, month);
            Amount = enrollment.MonthlyFee;

            DueDate = new DateTime(year, month, enrollment.DueDay);

            Status = TuitionStatus.Pending;
        }

        public void RegisterPayment(
            DateTime paidAt,
            decimal amount,
            PaymentMethod method)
        {
            if (Status == TuitionStatus.Paid)
                throw new InvalidOperationException(
                    "Tuition is already paid");

            if (amount <= 0)
                throw new ArgumentException(
                    "Amount must be greater than zero",
                    nameof(amount));

            if (!Enum.IsDefined(method))
                throw new ArgumentException(
                    "Invalid payment method",
                    nameof(method));

            Status = TuitionStatus.Paid;
            PaidAt = paidAt.Date;
            PaidAmount = amount;
            Method = method;
        }

        public static bool TryParseReferenceMonth(
            string? value,
            out int year,
            out int month)
        {
            year = 0;
            month = 0;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            var text = value.Trim();

            if (text.Length != ReferenceMonthLength || text[4] != '-')
                return false;

            if (!int.TryParse(
                    text.AsSpan(0, 4),
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out year) ||
                !int.TryParse(
                    text.AsSpan(5, 2),
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out month))
            {
                return false;
            }

            return year >= 1 && month >= 1 && month <= 12;
        }

        public static string FormatReferenceMonth(int year, int month)
        {
            return $"{year:D4}-{month:D2}";
        }
    }
}