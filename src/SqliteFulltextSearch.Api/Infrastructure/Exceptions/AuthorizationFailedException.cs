// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Api.Infrastructure.Errors;

namespace ElasticsearchFulltextExample.Api.Infrastructure.Exceptions
{
    public class AuthorizationFailedException : ApplicationErrorException
    {
        /// <inheritdoc/>
        public override string ErrorCode => ErrorCodes.AuthorizationFailed;

        /// <inheritdoc/>
        public override string ErrorMessage => $"AuthorizationFailed";

        /// <inheritdoc/>
        public override int HttpStatusCode => StatusCodes.Status403Forbidden;

        /// <summary>
        /// Creates a new <see cref="AuthorizationFailedException"/>.
        /// </summary>
        /// <param name="message">Error Message</param>
        /// <param name="innerException">Reference to the Inner Exception</param>
        public AuthorizationFailedException(string message, Exception? innerException = null)
            : base(message, innerException)
        {
        }
    }
}
