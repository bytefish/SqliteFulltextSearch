// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Api.Infrastructure.Exceptions;
using ElasticsearchFulltextExample.Api.Models;
using ElasticsearchFulltextExample.Shared.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ElasticsearchFulltextExample.Api.Infrastructure.Errors.Translators
{
    public class ApplicationErrorExceptionTranslator : IExceptionTranslator
    {
        private readonly ILogger<ApplicationErrorExceptionTranslator> _logger;

        public ApplicationErrorExceptionTranslator(ILogger<ApplicationErrorExceptionTranslator> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public IResult GetApplicationErrorResult(Exception exception, bool includeExceptionDetails)
        {
            _logger.TraceMethodEntry();

            var applicationErrorException = (ApplicationErrorException)exception;

            return InternalGetApplicationErrorResult(applicationErrorException, includeExceptionDetails);
        }

        private IResult InternalGetApplicationErrorResult(ApplicationErrorException exception, bool includeExceptionDetails)
        {
            var error = new ApplicationError
            {
                Code = exception.ErrorCode,
                Message = exception.ErrorMessage,
            };

            error.InnerError = new ApplicationInnerError();

            // Create the Inner Error
            if (includeExceptionDetails)
            {
                error.InnerError.Message = exception.Message;
                error.InnerError.StackTrace = exception.StackTrace;
                error.InnerError.Target = exception.GetType().Name;
            }

            return TypedResults.Json(error, statusCode: exception.HttpStatusCode);
        }

        /// <inheritdoc/>
        public Type ExceptionType => typeof(ApplicationErrorException);
    }
}
