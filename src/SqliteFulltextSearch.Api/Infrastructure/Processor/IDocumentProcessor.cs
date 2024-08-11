using SqliteFulltextSearch.Shared.Infrastructure;
using SqliteFulltextSearch.Api.Infrastructure.Pdf;
using SqliteFulltextSearch.Database.Model;
using SqliteFulltextSearch.Api.Infrastructure.Word;

namespace SqliteFulltextSearch.Api.Infrastructure.Processor
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

            if(!_documentProcessors.TryGetValue(documentExtension, out var documentProcessor))
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
                foreach(var supportedExtension in documentProcessor.SupportedExtensions)
                {
                    result.Add(supportedExtension, documentProcessor);
                }
            }

            return result;
        }


    }

    public interface IDocumentProcessor
    {
        public ValueTask<FtsDocument> ProcessDocumentAsync(Document document, CancellationToken cancellationToken);

        public string[] SupportedExtensions { get; }
    }

    public class PdfDocumentProcessor : IDocumentProcessor
    {
        private readonly ILogger<PdfDocumentProcessor> _logger;
        private readonly PdfDocumentReader _pdfDocumentReader;

        public PdfDocumentProcessor(ILogger<PdfDocumentProcessor> logger, PdfDocumentReader pdfDocumentReader)
        {
            _logger = logger;
            _pdfDocumentReader = pdfDocumentReader;
        }

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

        public string[] SupportedExtensions => [ ".pdf" ];

    }

    public class WordDocumentProcessor : IDocumentProcessor
    {
        private readonly ILogger<WordDocumentProcessor> _logger;
        private readonly WordDocumentReader _wordDocumentReader;

        public WordDocumentProcessor(ILogger<WordDocumentProcessor> logger, WordDocumentReader wordDocumentReader)
        {
            _logger = logger;
            _wordDocumentReader = wordDocumentReader;
        }

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

        public string[] SupportedExtensions => [".docx"];
    }

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