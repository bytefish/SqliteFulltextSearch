// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Diagnostics;
using SqliteFulltextSearch.Shared.Infrastructure;

namespace SqliteFulltextSearch.Api.Infrastructure.Errors
{
    public class ApplicationErrorExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<ApplicationErrorExceptionHandler> _logger;

        private readonly ExceptionToErrorMapper _exceptionToErrorMapper;

        public ApplicationErrorExceptionHandler(ILogger<ApplicationErrorExceptionHandler> logger, ExceptionToErrorMapper exceptionToErrorMapper)
        {
            _logger = logger;
            _exceptionToErrorMapper = exceptionToErrorMapper;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var applicationError = _exceptionToErrorMapper.CreateApplicationErrorResult(context, exception);

            await applicationError.ExecuteAsync(context);

            return true;
        }
    }
}
