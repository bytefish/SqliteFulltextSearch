// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using SqliteFulltextSearch.Api.Models;
using SqliteFulltextSearch.Database.Model;
using SqliteFulltextSearch.Shared.Infrastructure;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace SqliteFulltextSearch.Api.Infrastructure.DocumentProcessing.Readers
{
    /// <summary>
    /// Reads a PDF document and its metadata.
    /// </summary>
    public class PdfDocumentReader
    {
        private readonly ILogger<PdfDocumentReader> _logger;

        public PdfDocumentReader(ILogger<PdfDocumentReader> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Extracts the Metadata from a PDF Document.
        /// </summary>
        /// <param name="document">Document with the data</param>
        /// <returns>Metadata of the document</returns>
        public DocumentMetadata ExtractMetadata(Document document)
        {
            _logger.TraceMethodEntry();

            // Read as PDF Document
            using PdfDocument pdfDocument = PdfDocument.Open(document.Data);

            var content = ReadAllText(pdfDocument);

            return new DocumentMetadata
            {
                Author = pdfDocument.Information.Author,
                Content = content,
                CreationDate = pdfDocument.Information.CreationDate,
                Creator = pdfDocument.Information.Creator,
                Subject = pdfDocument.Information.Subject,
                Title = pdfDocument.Information.Title,
            };
        }

        private string ReadAllText(PdfDocument pdfDocument)
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (Page page in pdfDocument.GetPages())
            {
                var text = ContentOrderTextExtractor.GetText(page);

                stringBuilder.AppendLine(text);
            }

            return stringBuilder.ToString();

        }

    }
}
