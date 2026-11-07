using System.Data;
using Domain.Exceptions;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public async Task<T> ExecuteSerializableAsync<T>(Func<Task<T>> action)
        {
            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var result = await action();
                await transaction.CommitAsync();
                return result;
            }
            catch (Exception ex) when (PostgresErrors.Has(
                ex,
                PostgresErrors.SerializationFailure,
                PostgresErrors.DeadlockDetected,
                PostgresErrors.UniqueViolation))
            {
                throw new ConcurrencyConflictException(
                    "The operation conflicts with a concurrent request. Please try again.",
                    ex);
            }
        }
    }
}
