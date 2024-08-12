// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using SqliteFulltextSearch.Shared.Infrastructure;
using SqliteFulltextSearch.Database.Model;
using SqliteFulltextSearch.Api.Infrastructure.DocumentProcessing.Readers;

namespace SqliteFulltextSearch.Api.Infrastructure.DocumentProcessing.Processors
{
    /// <summary>
    /// Processes a PDF File using PdfPig.
    /// </summary>
    public class PdfDocumentProcessor : IDocumentProcessor
    {
        private readonly ILogger<PdfDocumentProcessor> _logger;
        private readonly PdfDocumentReader _pdfDocumentReader;

        public PdfDocumentProcessor(ILogger<PdfDocumentProcessor> logger, PdfDocumentReader pdfDocumentReader)
        {
            _logger = logger;
            _pdfDocumentReader = pdfDocumentReader;
        }

        /// <inheritdoc/>
        public ValueTask<FtsDocument> ProcessDocumentAsync(Document document, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            // Get the PDF Metadata:
            var metadata = _pdfDocumentReader.ExtractMetadata(document);

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
        public string[] SupportedExtensions => [".pdf"];
    }
}