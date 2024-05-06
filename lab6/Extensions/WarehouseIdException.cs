using System.Net.Mime;
using Microsoft.AspNetCore.Diagnostics;

namespace lab6.Extensions
{

    public class WarehouseIdException : Exception, IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is WarehouseIdException)
            {
                httpContext.Response.StatusCode = 404; 
                httpContext.Response.ContentType = MediaTypeNames.Text.Html;
                await httpContext.Response.WriteAsync("<p>There is no Warehouse with that id</p>");
                return true; 
            }

            return false; 
        }
    }
}