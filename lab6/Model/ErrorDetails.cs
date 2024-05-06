using System.Text.Json;

namespace lab6.Model
{

// TODO, try to implement Handling Errors Globally With the Custom Middleware


    public class ErrorDetails
    {
   
            public int StatusCode { get; set; }
            public string Message { get; set; }
            public override string ToString()
            {
                return JsonSerializer.Serialize(this);
            }
        
    }
}