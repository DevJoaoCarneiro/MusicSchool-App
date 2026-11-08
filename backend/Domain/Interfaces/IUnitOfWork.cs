namespace Domain.Interfaces
{
    public interface IUnitOfWork
    {
        Task<T> ExecuteSerializableAsync<T>(Func<Task<T>> action);
    }
}
