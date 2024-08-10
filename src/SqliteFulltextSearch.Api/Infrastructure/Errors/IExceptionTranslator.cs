// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Http.HttpResults;
using SqliteFulltextSearch.Api.Models;
using System;

namespace SqliteFulltextSearch.Api.Infrastructure.Errors
{
    /// <summary>
    /// A Translator to convert from an <see cref="Exception"/> to an <see cref="ApplicationErrorResult"/>.
    /// </summary>
    public interface IExceptionTranslator
    {
        /// <summary>
        /// Translates a given <see cref="Exception"/> into an <see cref="ApplicationErrorResult"/>.
        /// </summary>
        /// <param name="exception">Exception to translate</param>
        /// <param name="includeExceptionDetails">A flag, if exception details should be included</param>
        /// <returns>The <see cref="ApplicationErrorResult"/> for the <see cref="Exception"/></returns>
        JsonHttpResult<ApplicationError> GetApplicationErrorResult(Exception exception, bool includeExceptionDetails);

        /// <summary>
        /// Gets or sets the Exception Type.
        /// </summary>
        Type ExceptionType { get; }
    }
}
