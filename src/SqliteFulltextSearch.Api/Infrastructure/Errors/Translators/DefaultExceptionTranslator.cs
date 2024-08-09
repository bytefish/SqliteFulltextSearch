// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Api.Models;
using ElasticsearchFulltextExample.Shared.Infrastructure;

namespace ElasticsearchFulltextExample.Api.Infrastructure.Errors.Translators
{
    public class DefaultExceptionTranslator : IExceptionTranslator
    {
        private readonly ILogger<DefaultExceptionTranslator> _logger;

        public DefaultExceptionTranslator(ILogger<DefaultExceptionTranslator> logger)
        {
            _logger = logger;
        }

        public Type ExceptionType => typeof(Exception);

        public ApplicationErrorResult GetApplicationErrorResult(Exception exception, bool includeExceptionDetails)
        {
            _logger.TraceMethodEntry();

            var error = new ApplicationError
            {
                Code = ErrorCodes.InternalServerError,
                Message = "An Internal Server Error occured"
            };

            // Create the Inner Error
            error.InnerError = new ApplicationInnerError();

            if (includeExceptionDetails)
            {
                error.InnerError.Message = exception.Message;
                error.InnerError.StackTrace = exception.StackTrace;
                error.InnerError.Target = exception.GetType().Name;
            }

            return new ApplicationErrorResult
            {
                Error = error,
                StatusCode = StatusCodes.Status500InternalServerError,
            };
        }
    }
}
