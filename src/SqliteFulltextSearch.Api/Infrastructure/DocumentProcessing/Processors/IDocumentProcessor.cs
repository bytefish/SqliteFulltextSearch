// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using SqliteFulltextSearch.Database.Model;

namespace SqliteFulltextSearch.Api.Infrastructure.DocumentProcessing.Processors
{
    /// <summary>
    /// Processes a Document and prepares an entry for the Fulltext Search table.
    /// </summary>
    public interface IDocumentProcessor
    {
        /// <summary>
        /// Processes the given document.
        /// </summary>
        /// <param name="document">Document with the data included</param>
        /// <param name="cancellationToken">Cancellation Token to cancel asynchronous processing</param>
        /// <returns>The entry for the FTS table</returns>
        public ValueTask<FtsDocument> ProcessDocumentAsync(Document document, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the list of File types supported by the processor.
        /// </summary>
        public string[] SupportedExtensions { get; }
    }
}