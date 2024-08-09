// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Client.Localization;
using Microsoft.Extensions.Localization;

namespace ElasticsearchFulltextExample.Web.Client.Infrastructure
{
    public class ApplicationErrorTranslator
    {
        private readonly IStringLocalizer<SharedResource> _sharedLocalizer;

        public ApplicationErrorTranslator(IStringLocalizer<SharedResource> sharedLocalizer)
        {
            _sharedLocalizer = sharedLocalizer;
        }

        public (string ErrorCode, string ErrorMessage) GetErrorMessage(Exception exception)
        {
            return exception switch
            {
                Exception e => (LocalizationConstants.ClientError_UnexpectedError, GetErrorMessageFromException(e)),
            };
        }

        private string GetErrorMessageFromException(Exception e)
        {
            string errorMessage = _sharedLocalizer["ApplicationError_Exception"];

            return errorMessage;
        }
    }
}
