using UglyToad.PdfPig.Content;
using UglyToad.PdfPig;
using SqliteFulltextSearch.Database.Model;
using System.Text;
using SqliteFulltextSearch.Api.Models;

namespace SqliteFulltextSearch.Api.Services
{
    public class PdfDocumentService
    {
        private readonly ILogger<PdfDocumentService> _logger;

        public PdfDocumentService(ILogger<PdfDocumentService> logger)
        {
            _logger = logger;
        }

        public DocumentMetadata ExtractMetadata(Document document)
        {
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
                string pageText = page.Text;

                stringBuilder.AppendLine(pageText);
            }

            return stringBuilder.ToString();
        }
    }
}
