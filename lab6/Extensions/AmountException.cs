using System.Net.Mime;
using Microsoft.AspNetCore.Diagnostics;

namespace lab6.Extensions
{



    public class AmountException : Exception, IExceptionHandler
    {

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is AmountException)
            {
                httpContext.Response.StatusCode = 400; 
                httpContext.Response.ContentType = MediaTypeNames.Text.Html;
                await httpContext.Response.WriteAsync("<p>Amount must be greater than 0</p>");
                return true; 
            }

            return false; 
        }
    }
}