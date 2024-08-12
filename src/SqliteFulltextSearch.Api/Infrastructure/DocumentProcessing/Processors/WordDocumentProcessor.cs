// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using SqliteFulltextSearch.Shared.Infrastructure;
using SqliteFulltextSearch.Database.Model;
using SqliteFulltextSearch.Api.Infrastructure.DocumentProcessing.Readers;

namespace SqliteFulltextSearch.Api.Infrastructure.DocumentProcessing.Processors
{
    /// <summary>
    /// Processes a Word Document using OpenXml.
    /// </summary>
    public class WordDocumentProcessor : IDocumentProcessor
    {
        private readonly ILogger<WordDocumentProcessor> _logger;
        private readonly WordDocumentReader _wordDocumentReader;

        public WordDocumentProcessor(ILogger<WordDocumentProcessor> logger, WordDocumentReader wordDocumentReader)
        {
            _logger = logger;
            _wordDocumentReader = wordDocumentReader;
        }

        /// <inheritdoc/>
        public ValueTask<FtsDocument> ProcessDocumentAsync(Document document, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var metadata = _wordDocumentReader.ExtractMetadata(document);

            // Create the Search Document
            var ftsDocument = new FtsDocument
            {
                RowId = document.Id,
                Content = metadata.Content ?? string.Empty,
                Title = document.Title,
            };

            return ValueTask.FromResult(ftsDocument);
        }

        /// <inheritdoc/>
        public string[] SupportedExtensions => [".docx"];
    }
}