// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using SqliteFulltextSearch.Api.Models;
using SqliteFulltextSearch.Database.Model;
using SqliteFulltextSearch.Shared.Infrastructure;
using System.Text;
using UtfUnknown;

namespace SqliteFulltextSearch.Api.Infrastructure.DocumentProcessing.Readers
{
    /// <summary>
    /// Reads a text document and its metadata.
    /// </summary>
    public class TextDocumentReader
    {
        private readonly ILogger<TextDocumentReader> _logger;

        public TextDocumentReader(ILogger<TextDocumentReader> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Extracts the Metadata from a Text Document.
        /// </summary>
        /// <param name="document">Document with the data</param>
        /// <returns>Metadata of the document</returns>
        public DocumentMetadata ExtractMetadata(Document document)
        {
            _logger.TraceMethodEntry();
            
            var detectionResult = CharsetDetector.DetectFromBytes(document.Data);

            var encoding = detectionResult.Detected.Encoding;
            
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            var content = encoding.GetString(document.Data);
            
            return new DocumentMetadata
            {
                Content = content,
            };
        }
    }
}
