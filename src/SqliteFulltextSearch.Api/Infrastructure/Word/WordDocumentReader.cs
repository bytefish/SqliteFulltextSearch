using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Mvc.Formatters.Xml;
using SqliteFulltextSearch.Api.Models;
using SqliteFulltextSearch.Database.Model;
using SqliteFulltextSearch.Shared.Infrastructure;
using System.Globalization;
using System.Text;

namespace SqliteFulltextSearch.Api.Infrastructure.Word
{
    public class WordDocumentReader
    {
        private readonly ILogger<WordDocumentReader> _logger;

        public WordDocumentReader(ILogger<WordDocumentReader> logger)
        {
            _logger = logger;
        }

        public DocumentMetadata ExtractMetadata(Document document)
        {
            _logger.TraceMethodEntry();

            using (var ms = new MemoryStream(document.Data))
            {
                using(var wpd = WordprocessingDocument.Open(ms, true)) 
                {
                    var element = wpd.MainDocumentPart?.Document.Body;

                    if(element == null)
                    {
                        return new DocumentMetadata();
                    }

                    var content = GetAsPlainText(element);

                    return new DocumentMetadata
                    {
                        Author = wpd.PackageProperties.Creator, // Not really, right?
                        Title = wpd.PackageProperties.Title,
                        Subject = wpd.PackageProperties.Subject,
                        Creator = wpd.PackageProperties.Creator,
                        Content = content,
                        CreationDate = wpd.PackageProperties.Created?.ToString(CultureInfo.InvariantCulture),
                    };
                }
            }
        }

        public string GetAsPlainText(OpenXmlElement element)
        {
            StringBuilder stringBuilder = new StringBuilder();
            
            foreach(var section in element.Elements())
            {
                switch(section.LocalName)
                {
                    case "t":
                        stringBuilder.Append(section.InnerText); 
                        break;
                    case "cr":
                    case "br":
                        stringBuilder.Append('\n');
                        break;
                    case "tab":
                        stringBuilder.Append('\t');
                        break;
                    case "p":
                        stringBuilder.Append(GetAsPlainText(section));
                        stringBuilder.AppendLine(Environment.NewLine);
                        break;
                    default:
                        stringBuilder.Append(GetAsPlainText(section));
                        break;
                }
            }

            return stringBuilder.ToString();
        }
    }
}
