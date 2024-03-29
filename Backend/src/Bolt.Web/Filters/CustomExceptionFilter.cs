﻿namespace Bolt.Web.Filters
{
    using System;
    using System.Net;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Filters;

    public class CustomExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            HttpStatusCode status = HttpStatusCode.InternalServerError;
            string message = string.Empty;

            Type exceptionType = context.Exception.GetType();

            if (exceptionType == typeof(NotImplementedException))
            {
                message = "A server error occurred.";
                status = HttpStatusCode.NotImplemented;
            }
            else
            {
                message = context.Exception.Message;
                status = HttpStatusCode.NotFound;
            }

            HttpResponse response = context.HttpContext.Response;

            response.StatusCode = (int)status;

            response.ContentType = "application/json";

            string err = message + " " + context.Exception.StackTrace;

            response.WriteAsync(err);
        }
    }
}
