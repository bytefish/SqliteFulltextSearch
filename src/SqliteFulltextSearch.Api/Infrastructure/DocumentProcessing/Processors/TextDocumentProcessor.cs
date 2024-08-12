using SqliteFulltextSearch.Shared.Infrastructure;
using SqliteFulltextSearch.Database.Model;

namespace SqliteFulltextSearch.Api.Infrastructure.DocumentProcessing.Processors
{
    public class TextDocumentProcessor : IDocumentProcessor
    {
        private readonly ILogger<TextDocumentProcessor> _logger;

        public TextDocumentProcessor(ILogger<TextDocumentProcessor> logger)
        {
            _logger = logger;
        }

        public ValueTask<FtsDocument> ProcessDocumentAsync(Document document, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            // Supposes the Text is given in UTF-8:
            var content = System.Text.Encoding.UTF8.GetString(document.Data);

            // Create the Search Document
            var ftsDocument = new FtsDocument
            {
                RowId = document.Id,
                Content = content,
                Title = document.Title,
            };

            return ValueTask.FromResult(ftsDocument);
        }

        public string[] SupportedExtensions => ["txt", "htm", ".html", ".md"];

    }
}