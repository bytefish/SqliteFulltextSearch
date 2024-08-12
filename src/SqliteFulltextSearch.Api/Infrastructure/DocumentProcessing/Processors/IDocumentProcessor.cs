using SqliteFulltextSearch.Database.Model;

namespace SqliteFulltextSearch.Api.Infrastructure.DocumentProcessing.Processors
{

    public interface IDocumentProcessor
    {
        public ValueTask<FtsDocument> ProcessDocumentAsync(Document document, CancellationToken cancellationToken);

        public string[] SupportedExtensions { get; }
    }
}