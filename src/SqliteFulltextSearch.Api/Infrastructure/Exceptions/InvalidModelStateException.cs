// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Api.Infrastructure.Errors;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ElasticsearchFulltextExample.Api.Infrastructure.Exceptions
{
    public class InvalidModelStateException : ApplicationErrorException
    {
        /// <inheritdoc/>
        public override string ErrorCode => ErrorCodes.ValidationFailed;

        /// <inheritdoc/>
        public override string ErrorMessage => $"ValidationFailure";

        /// <inheritdoc/>
        public override int HttpStatusCode => StatusCodes.Status400BadRequest;

        /// <summary>
        /// Gets or sets the ModelStateDictionary.
        /// </summary>
        public required ModelStateDictionary ModelStateDictionary { get; set; }

        /// <summary>
        /// Creates a new <see cref="InvalidModelStateException"/>.
        /// </summary>
        /// <param name="message">Error Message</param>
        /// <param name="innerException">Reference to the Inner Exception</param>
        public InvalidModelStateException(string message = "InvalidModelState", Exception? innerException = null)
            : base(message, innerException)
        {
        }
    }
}
