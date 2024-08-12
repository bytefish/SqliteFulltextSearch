// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using SqliteFulltextSearch.Shared.Infrastructure;
using SqliteFulltextSearch.Database.Model;
using SqliteFulltextSearch.Api.Infrastructure.DocumentProcessing.Readers;

namespace SqliteFulltextSearch.Api.Infrastructure.DocumentProcessing.Processors
{
    /// <summary>
    /// Returns the File content returned by a File.ReadAllText. It is assumed, that the 
    /// text file is UTF8 encoded, which might be problematic.
    /// </summary>
    public class TextDocumentProcessor : IDocumentProcessor
    {
        private readonly ILogger<TextDocumentProcessor> _logger;
        
        private readonly TextDocumentReader _textDocumentReader;

        public TextDocumentProcessor(ILogger<TextDocumentProcessor> logger, TextDocumentReader textDocumentReader)
        {
            _logger = logger;
            _textDocumentReader = textDocumentReader;
        }

        /// <inheritdoc/>
        public ValueTask<FtsDocument> ProcessDocumentAsync(Document document, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            // Supposes the Text is given in UTF-8:
            var metadata = _textDocumentReader.ExtractMetadata(document);

            var content = metadata.Content;

            if (content == null)
            {
                content = string.Empty;
            }

            var ftsDocument = new FtsDocument
            {
                RowId = document.Id,
                Content = content,
                Title = document.Title,
            };

            return ValueTask.FromResult(ftsDocument);
        }

        /// <inheritdoc/>
        public string[] SupportedExtensions => [".txt", ".htm", ".html", ".md"];
    }
}