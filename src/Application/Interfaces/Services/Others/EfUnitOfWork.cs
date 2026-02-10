namespace Bookazone.Application.Interfaces.Services.Others;

public interface IEfUnitOfWork
{
    Task ExecuteAsync(Func<Task> action);
}
