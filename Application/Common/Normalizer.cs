namespace Application.Common
{
    public static class Normalizer
    {
        public static string Email(string email) =>
            email.Trim().ToLowerInvariant();

        public static DateTime LocalDateTime(DateTime value) =>
            value.Kind == DateTimeKind.Utc
                ? DateTime.SpecifyKind(value.ToLocalTime(), DateTimeKind.Unspecified)
                : DateTime.SpecifyKind(value, DateTimeKind.Unspecified);

        public static DateTime? LocalDateTime(DateTime? value) =>
            value.HasValue ? LocalDateTime(value.Value) : null;
    }
}
