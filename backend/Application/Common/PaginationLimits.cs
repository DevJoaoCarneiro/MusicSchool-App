namespace Application.Common
{
    public static class PaginationLimits
    {
        public const int MaxPageSize = 100;

        public static readonly string InvalidMessage =
            $"A página deve ser maior que zero e o tamanho da página deve estar entre 1 e {MaxPageSize}";

        public static bool IsValid(int page, int pageSize) =>
            page >= 1 && pageSize >= 1 && pageSize <= MaxPageSize;
    }
}
