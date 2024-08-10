// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using SqliteFulltextSearch.Api.Infrastructure.Exceptions;
using SqliteFulltextSearch.Api.Models;
using SqliteFulltextSearch.Shared.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http.HttpResults;

namespace SqliteFulltextSearch.Api.Infrastructure.Errors
{
    /// <summary>
    /// Handles errors returned by the application.
    /// </summary>
    public class ExceptionToErrorMapper
    {
        private readonly ILogger<ExceptionToErrorMapper> _logger;

        private readonly ExceptionToErrorMapperOptions _options;
        private readonly Dictionary<Type, IExceptionTranslator> _translators;

        public ExceptionToErrorMapper(ILogger<ExceptionToErrorMapper> logger, IOptions<ExceptionToErrorMapperOptions> options, IEnumerable<IExceptionTranslator> translators)
        {
            _logger = logger;
            _options = options.Value;
            _translators = translators.ToDictionary(x => x.ExceptionType, x => x);
        }

        public JsonHttpResult<ApplicationError> CreateApplicationErrorResult(HttpContext httpContext, Exception exception)
        {
            _logger.TraceMethodEntry();

            // Get the best matching translator for the exception ...
            var translator = GetTranslator(exception);

            // ... translate it to the Result ...
            var error = translator.GetApplicationErrorResult(exception, _options.IncludeExceptionDetails);

            // ... add error metadata, such as a Trace ID, ...
            AddMetadata(httpContext, error);

            // ... and return it.
            return error;
        }

        private void AddMetadata(HttpContext httpContext, JsonHttpResult<ApplicationError> result)
        {
            if(result.Value == null)
            {
                return;
            }

            if (result.Value.InnerError == null)
            {
                result.Value.InnerError = new ApplicationInnerError();
            }

            result.Value.InnerError.AdditionalProperties["trace-id"] = httpContext.TraceIdentifier;
        }

        private IExceptionTranslator GetTranslator(Exception e)
        {
            if (e is ApplicationErrorException)
            {
                if (_translators.TryGetValue(e.GetType(), out var translator))
                {
                    return translator;
                }

                return _translators[typeof(ApplicationErrorException)];
            }

            return _translators[typeof(Exception)];
        }
    }
}