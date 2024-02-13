using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;

namespace ExternalApi.Filters
{
    public sealed class HandleExceptionAttribute : ExceptionFilterAttribute
    {
        public ILogger<HandleExceptionAttribute> Logger { get; private set; }
        public HandleExceptionAttribute(ILogger<HandleExceptionAttribute> logger)
        {
            Logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            try
            {
                Logger.LogError(context.Exception, context.Exception.Message);
            }
            catch { }

            if (context.Exception != null)
            {
                context.Result = new ContentResult
                {
                    Content = JsonConvert.SerializeObject(new { ErrorType = "UNEXPECTED", InError = true, Messages = new List<string> { { "UNEXPECTED_ERROR" } } }),
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    ContentType = "application/json"
                };
                context.ExceptionHandled = true;
            }
        }
    }
}
