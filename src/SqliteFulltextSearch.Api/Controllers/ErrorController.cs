// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Api.Infrastructure.Errors;
using ElasticsearchFulltextExample.Api.Models;
using ElasticsearchFulltextExample.Shared.Infrastructure;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ElasticsearchFulltextExample.Api.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        private readonly ILogger<ErrorController> _logger;

        private readonly ExceptionToErrorMapper _exceptionToApplicationErrorMapper;

        public ErrorController(ILogger<ErrorController> logger, ExceptionToErrorMapper exceptionToODataErrorMapper)
        {
            _logger = logger;
            _exceptionToApplicationErrorMapper = exceptionToODataErrorMapper;
        }

        [Route("/error")]
        public IActionResult HandleError()
        {
            _logger.TraceMethodEntry();

            var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>()!;

            var error = _exceptionToApplicationErrorMapper.CreateApplicationErrorResult(HttpContext, exceptionHandlerFeature.Error);

            return new JsonResult(error)
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        [Route("/error/401")]
        public IActionResult HandleHttpStatus401()
        {
            _logger.TraceMethodEntry();

            var error = new ApplicationError
            {
                Code = ErrorCodes.Unauthorized,
                Message = "Unauthorized"
            };

            error.InnerError = new ApplicationInnerError();

            error.InnerError.AdditionalProperties["trace-id"] = HttpContext.TraceIdentifier;

            return new JsonResult(error)
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
        }

        [Route("/error/404")]
        public IActionResult HandleHttpStatus404()
        {
            _logger.TraceMethodEntry();

            var error = new ApplicationError
            {
                Code = ErrorCodes.ResourceNotFound,
                Message = "ResourceNotFound"
            };

            error.InnerError = new ApplicationInnerError();
            error.InnerError.AdditionalProperties["trace-id"] = HttpContext.TraceIdentifier;

            return new JsonResult(error)
            {
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        [Route("/error/405")]
        public IActionResult HandleHttpStatus405()
        {
            _logger.TraceMethodEntry();

            var error = new ApplicationError
            {
                Code = ErrorCodes.MethodNotAllowed,
                Message = "MethodNotAllowed"
            };

            error.InnerError = new ApplicationInnerError();
            error.InnerError.AdditionalProperties["trace-id"] = HttpContext.TraceIdentifier;

            return new JsonResult(error)
            {
                StatusCode = StatusCodes.Status405MethodNotAllowed
            };
        }

        [Route("/error/429")]
        public IActionResult HandleHttpStatus429()
        {
            _logger.TraceMethodEntry();

            var error = new ApplicationError
            {
                Code = ErrorCodes.TooManyRequests,
                Message = "TooManyRequests"
            };

            error.InnerError = new ApplicationInnerError();
            error.InnerError.AdditionalProperties["trace-id"] = HttpContext.TraceIdentifier;

            return new JsonResult(error)
            {
                StatusCode = StatusCodes.Status429TooManyRequests
            };
        }
    }
}