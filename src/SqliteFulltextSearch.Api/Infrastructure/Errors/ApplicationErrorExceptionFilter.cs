
namespace ElasticsearchFulltextExample.Api.Infrastructure.Errors
{
    public class ApplicationErrorExceptionFilter : IEndpointFilter
    {
        private readonly ILogger<ApplicationErrorExceptionFilter> _logger;
        private readonly ExceptionToErrorMapper _exceptionToErrorMapper;

        public ApplicationErrorExceptionFilter(ILogger<ApplicationErrorExceptionFilter> logger, ExceptionToErrorMapper exceptionToErrorMapper)
        {
            _logger = logger;
            _exceptionToErrorMapper = exceptionToErrorMapper;
        }

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            try
            {
                return await next(context);
            } 
            catch(Exception e)
            {
                _exceptionToErrorMapper.CreateApplicationErrorResult(context.HttpContext, e);
            }

        }
    }
}
