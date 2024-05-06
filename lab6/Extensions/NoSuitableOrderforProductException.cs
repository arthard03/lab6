using System.Net.Mime;
using Microsoft.AspNetCore.Diagnostics;

namespace lab6.Extensions
{
    


public class NoSuitableOrderforProductException : Exception, IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is NoSuitableOrderforProductException)
        {
            httpContext.Response.StatusCode = 404; 
            httpContext.Response.ContentType = MediaTypeNames.Text.Html;
            await httpContext.Response.WriteAsync("<p>No suitable Order for Product</p>");
            return true;
        }

        return false; 
    }
}}