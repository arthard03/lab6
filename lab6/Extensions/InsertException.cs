using System.Net.Mime;
using Microsoft.AspNetCore.Diagnostics;

namespace lab6.Extensions
{
    


public class InsertException : Exception, IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is InsertException)
        {
            httpContext.Response.StatusCode = 500; 
            httpContext.Response.ContentType = MediaTypeNames.Text.Html;
            await httpContext.Response.WriteAsync("<p>Insert error</p>");
            return true; 
        }

        return false; 
    }
}}