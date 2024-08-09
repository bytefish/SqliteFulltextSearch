using System.Text.Json.Serialization;

namespace ElasticsearchFulltextExample.Api.Models
{
    public class SearchSuggestion
    {
        public required string Text { get; set; }

        public required string Highlight { get; set; }
    }
}
