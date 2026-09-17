public class UserContextMiddleware
{
    private readonly RequestDelegate _next;

    public UserContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, IUserContext userContext)
    {
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            // userContext.UserId = httpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value 
            //                   ?? httpContext.User.FindFirst("oid")?.Value;
            userContext.UserId = httpContext.User.FindFirst("oid")?.Value;

            userContext.Email = httpContext.User.FindFirst("preferred_username")?.Value 
                             ?? httpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
        }

        await _next(httpContext);
    }
}