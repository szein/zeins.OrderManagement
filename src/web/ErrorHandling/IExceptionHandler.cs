using System.Net;

public interface IExceptionHandler
{
    Task HandleAsync(Exception exception, string title, string operation);
}