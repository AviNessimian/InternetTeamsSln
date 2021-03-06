﻿using InternetTeams.Application.Exceptions;
using InternetTeams.Domain.Exceptions;
using InternetTeams.Persistence.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace InternetTeams.Web.Filters
{
    internal class ApiExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<ApiExceptionFilter> _logger;
        private readonly IDictionary<Type, Action<ExceptionContext>> _exceptionHandlers;

        public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
        {
            _logger = logger;

            // Register known exception types and handlers.
            _exceptionHandlers = new Dictionary<Type, Action<ExceptionContext>>
            {
                { typeof(DomainValidationException), HandleValidationException },
                { typeof(AppValidationException), HandleValidationException },
                { typeof(NotFoundException), HandleNotFoundException },
                { typeof(PersistenceException), HandlePersistenceException },
            };
        }

        public void OnException(ExceptionContext context)
        {
            context.HttpContext.Response.ContentType = new FileExtensionContentTypeProvider().Mappings[".json"];

            Type type = context.Exception.GetType();
            if (_exceptionHandlers.ContainsKey(type))
            {
                _exceptionHandlers[type].Invoke(context);
                return;
            }

            HandleUnknownException(context);
        }

        private void HandleUnknownException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, context.Exception.Message);

            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var details = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An error occurred while processing your request.",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            };

            context.Result = new JsonResult(new
            {
                //Details = details,
                ErrorMsg = HttpStatusCode.InternalServerError.ToString()
            });

            context.ExceptionHandled = true;
        }

        private void HandleValidationException(ExceptionContext context)
        {
            var ValidationEx = context.Exception as AppValidationException ?? context.Exception as DomainValidationException;
            if (ValidationEx != null)
            {
                _logger.LogInformation(ValidationEx, "Validation Failed!");
                foreach (var error in ValidationEx.Failures)
                {
                    context.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }

                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;

                var details = new ProblemDetails
                {
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Title = "",
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                };

                var response = JsonConvert.SerializeObject(new
                {
                    //Details = details,
                    Failures = ValidationEx.Failures,
                });

                context.Result = new JsonResult(response);
                context.ExceptionHandled = true;
            }
        }

        private void HandleNotFoundException(ExceptionContext context)
        {
            var notFoundEx = context.Exception as NotFoundException;
            if (notFoundEx != null)
            {
                _logger.LogInformation(notFoundEx, notFoundEx.Message);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                var response = JsonConvert.SerializeObject(new
                {
                    Message = notFoundEx.Message,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    Title = "The specified resource was not found.",
                });
                context.Result = new JsonResult(response);
                context.ExceptionHandled = true;
            }
        }

        private void HandlePersistenceException(ExceptionContext context)
        {
            var persistenceEx = context.Exception as PersistenceException;
            if (persistenceEx != null)
            {
                _logger.LogWarning(persistenceEx, persistenceEx.Message);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                var response = JsonConvert.SerializeObject(new
                {
                    Message = persistenceEx.Message,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.4",
                    Title = "",
                });
                context.Result = new JsonResult(response);
                context.ExceptionHandled = true;
            }
        }
    }
}
