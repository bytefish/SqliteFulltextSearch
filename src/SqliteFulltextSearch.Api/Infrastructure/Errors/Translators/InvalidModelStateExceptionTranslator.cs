// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using SqliteFulltextSearch.Api.Infrastructure.Exceptions;
using SqliteFulltextSearch.Api.Models;
using SqliteFulltextSearch.Shared.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http.HttpResults;

namespace SqliteFulltextSearch.Api.Infrastructure.Errors.Translators
{
    public class InvalidModelStateExceptionTranslator : IExceptionTranslator
    {
        private readonly ILogger<InvalidModelStateExceptionTranslator> _logger;

        public InvalidModelStateExceptionTranslator(ILogger<InvalidModelStateExceptionTranslator> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public JsonHttpResult<ApplicationError> GetApplicationErrorResult(Exception exception, bool includeExceptionDetails)
        {
            var invalidModelStateException = (InvalidModelStateException)exception;

            return InternalGetODataErrorResult(invalidModelStateException, includeExceptionDetails);
        }

        /// <inheritdoc/>
        public Type ExceptionType => typeof(InvalidModelStateException);

        private JsonHttpResult<ApplicationError> InternalGetODataErrorResult(InvalidModelStateException exception, bool includeExceptionDetails)
        {
            _logger.TraceMethodEntry();

            if (exception.ModelStateDictionary.IsValid)
            {
                throw new InvalidOperationException("Could not create an error response from a valid ModelStateDictionary");
            }

            ApplicationError error = new ApplicationError()
            {
                Code = ErrorCodes.ValidationFailed,
                Message = "One or more validation errors occured",
                Details = GetApplicationErrorDetails(exception.ModelStateDictionary),
            };

            // Create the Inner Error
            error.InnerError = new ApplicationInnerError();

            if (includeExceptionDetails)
            {
                error.InnerError.Message = exception.Message;
                error.InnerError.StackTrace = exception.StackTrace;
                error.InnerError.Target = exception.GetType().Name;
            }

            // If we have something like a Deserialization issue, the ModelStateDictionary has
            // a lower-level Exception. We cannot do anything sensible with exceptions, so 
            // we add it to the InnerError.
            var firstException = GetFirstException(exception.ModelStateDictionary);

            if (firstException != null)
            {
                _logger.LogWarning(firstException, "The ModelState contains an Exception, which has caused the invalid state");

                error.InnerError.InnerError = new ApplicationInnerError
                {
                    Message = firstException.Message,
                    StackTrace = firstException.StackTrace,
                    Target = firstException.GetType().Name,
                };
            }

            return TypedResults.Json(error, statusCode: StatusCodes.Status400BadRequest);
        }

        private Exception? GetFirstException(ModelStateDictionary modelStateDictionary)
        {
            _logger.TraceMethodEntry();

            foreach (var modelStateEntry in modelStateDictionary)
            {
                foreach (var modelError in modelStateEntry.Value.Errors)
                {
                    if (modelError.Exception != null)
                    {
                        return modelError.Exception;
                    }
                }
            }

            return null;
        }

        private List<ApplicationErrorDetail> GetApplicationErrorDetails(ModelStateDictionary modelStateDictionary)
        {
            _logger.TraceMethodEntry();

            var result = new List<ApplicationErrorDetail>();

            foreach (var modelStateEntry in modelStateDictionary)
            {
                foreach (var modelError in modelStateEntry.Value.Errors)
                {
                    // We cannot make anything sensible for the caller here. We log it, but then go on 
                    // as if nothing has happened. Alternative is to populate a chain of ODataInnerError 
                    // or abuse the ODataErrorDetails...
                    if (modelError.Exception != null)
                    {
                        continue;
                    }

                    // A ModelStateDictionary has nothing like an "ErrorCode" and it's not 
                    // possible with existing infrastructure to get an "ErrorCode" here. So 
                    // we set a generic one.
                    var errorCode = ErrorCodes.ValidationFailed;

                    var odataErrorDetail = new ApplicationErrorDetail
                    {
                        ErrorCode = errorCode,
                        Message = modelError.ErrorMessage,
                        Target = modelStateEntry.Key,
                    };

                    result.Add(odataErrorDetail);
                }
            }

            return result;
        }
    }
}
