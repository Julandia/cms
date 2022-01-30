using ConventionManagementService.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ConventionManagementService
{
    public class HttpResponseExceptionFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context) { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            ValidationException validationException;
            if (context.Exception != null && TryExtractValidationException(context.Exception, out validationException))
            {
                var errorResult = new ObjectResult(validationException.Message)
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
                context.ExceptionHandled = true;
                context.Result = errorResult;
            }
        }

        private static bool TryExtractValidationException(Exception error, out ValidationException validationException)
        {
            while (!(error is ValidationException))
            {
                if (error == null)
                {
                    validationException = null;
                    return false;
                }

                if (error is AggregateException)
                {
                    return TryExtractValidationException((AggregateException)error, out validationException);
                }

                error = error.InnerException;
            }

            validationException = (ValidationException)error;
            return true;
        }
    }
}
