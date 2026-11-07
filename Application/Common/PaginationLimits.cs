namespace Application.Common
{
    public static class PaginationLimits
    {
        public const int MaxPageSize = 100;

        public static readonly string InvalidMessage =
            $"Page must be greater than zero and pageSize must be between 1 and {MaxPageSize}";

        public static bool IsValid(int page, int pageSize) =>
            page >= 1 && pageSize >= 1 && pageSize <= MaxPageSize;
    }
}
