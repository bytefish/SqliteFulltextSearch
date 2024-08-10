// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace SqliteFulltextSearch.Api.Infrastructure.Errors
{
    /// <summary>
    /// Options for the <see cref="ExceptionToODataErrorMapper"/>.
    /// </summary>
    public class ExceptionToErrorMapperOptions
    {
        /// <summary>
        /// Gets or sets the option to include the Exception Details in the response.
        /// </summary>
        public bool IncludeExceptionDetails { get; set; } = false;
    }
}