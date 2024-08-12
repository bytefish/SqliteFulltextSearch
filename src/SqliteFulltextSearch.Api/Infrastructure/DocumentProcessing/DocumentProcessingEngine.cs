using SqliteFulltextSearch.Api.Infrastructure.DocumentProcessing.Processors;
using SqliteFulltextSearch.Database.Model;

namespace SqliteFulltextSearch.Api.Infrastructure.DocumentProcessing
{
    public class DocumentProcessingEngine
    {
        private readonly ILogger<DocumentProcessingEngine> _logger;
        private readonly IDictionary<string, IDocumentProcessor> _documentProcessors;

        public DocumentProcessingEngine(ILogger<DocumentProcessingEngine> logger, IEnumerable<IDocumentProcessor> documentProcessors)
        {
            _logger = logger;
            _documentProcessors = BuildLookupTable(documentProcessors);
        }

        public async ValueTask<FtsDocument> GetFtsDocumentAsync(Document document, CancellationToken cancellationToken)
        {
            var documentExtension = Path.GetExtension(document.Filename);

            if (!_documentProcessors.TryGetValue(documentExtension, out var documentProcessor))
            {
                return new FtsDocument
                {
                    RowId = document.Id,
                    Title = document.Title,
                    Content = string.Empty
                };
            }

            var ftsDocument = await documentProcessor
                .ProcessDocumentAsync(document, cancellationToken)
                .ConfigureAwait(false);

            return ftsDocument;
        }

        private IDictionary<string, IDocumentProcessor> BuildLookupTable(IEnumerable<IDocumentProcessor> documentProcessors)
        {
            var result = new Dictionary<string, IDocumentProcessor>();

            foreach (var documentProcessor in documentProcessors)
            {
                foreach (var supportedExtension in documentProcessor.SupportedExtensions)
                {
                    result.Add(supportedExtension, documentProcessor);
                }
            }

            return result;
        }
    }
}