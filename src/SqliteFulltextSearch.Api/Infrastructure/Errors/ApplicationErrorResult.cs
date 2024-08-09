using ElasticsearchFulltextExample.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace ElasticsearchFulltextExample.Api.Infrastructure.Errors
{
    /// <summary>
    /// Represents a result that when executed will produce an <see cref="ActionResult"/>.
    /// </summary>
    /// <remarks>This result creates an <see cref="ApplicationError"/> response.</remarks>
    public class ApplicationErrorResult : ActionResult
    {
        /// <summary>
        /// OData error.
        /// </summary>
        public ApplicationError Error { get; internal init; }

        /// <summary>
        /// Http Status Code.
        /// </summary>
        public int StatusCode { get; internal init; }

        internal ApplicationErrorResult(ApplicationError error, int? statusCode = null, string? contentType = null)
        {
            Error = error;
            StatusCode = statusCode;
        }


        /// <inheritdoc/>
        public async override Task ExecuteResultAsync(ActionContext context)
        {
            ObjectResult objectResult = new ObjectResult(Error)
            {
                StatusCode = StatusCode
            };

            await objectResult
                .ExecuteResultAsync(context)
                .ConfigureAwait(false);
        }
    }
}
