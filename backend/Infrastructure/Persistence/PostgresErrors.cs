using Npgsql;

namespace Infrastructure.Persistence
{
    internal static class PostgresErrors
    {
        public const string UniqueViolation = "23505";
        public const string SerializationFailure = "40001";
        public const string DeadlockDetected = "40P01";

        public static bool Has(Exception exception, params string[] sqlStates)
        {
            for (var current = exception; current != null; current = current.InnerException)
            {
                if (current is PostgresException postgres &&
                    sqlStates.Contains(postgres.SqlState))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
