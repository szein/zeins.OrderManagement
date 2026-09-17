using System.Net;
using Microsoft.Extensions.Logging;
using Radzen;

public sealed class ExceptionHandler : IExceptionHandler
{
    private readonly NotificationService notificationService;
    private readonly ILogger<ExceptionHandler> logger;

    public ExceptionHandler(NotificationService notificationService, ILogger<ExceptionHandler> logger)
    {
        this.notificationService = notificationService;
        this.logger = logger;
    }

    public Task HandleAsync(Exception exception, string title, string operation)
    {
        if (exception is HttpRequestException { StatusCode: HttpStatusCode.Forbidden })
        {
            notificationService.Notify(NotificationSeverity.Warning, "Access Denied", "You do not have permission to perform this action.");
        }
        else if (exception is HttpRequestException { StatusCode: HttpStatusCode.NotAcceptable })
        {
            notificationService.Notify(NotificationSeverity.Warning, "Action Denied", "This item is realted to an exsiting Order and it can not be changed.");
        }
        else if (exception is HttpRequestException { StatusCode: HttpStatusCode.Unauthorized })
        {
            notificationService.Notify(NotificationSeverity.Warning, "Access Denied", "You are not logged in.");
        }
        else
        {
            logger.LogError(exception, "Error while {Operation}", operation);
            notificationService.Notify(NotificationSeverity.Error, title, $"Could not {operation}: {exception.Message}");
        }

        return Task.CompletedTask;
    }
}