using System.Text.Json.Serialization;

namespace ElasticsearchFulltextExample.Shared.Models
{
    public class SearchSuggestionDto
    {
        [JsonPropertyName("text")]
        public required string Text { get; set; }


        [JsonPropertyName("highlight")]
        public required string Highlight { get; set; }
    }
}
