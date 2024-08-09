using System.Text.Json.Serialization;

namespace ElasticsearchFulltextExample.Api.Models
{
    public class SearchSuggestions
    {
        public required string Query { get; set; }


        [JsonPropertyName("results")]
        public List<SearchSuggestion> Results { get; set; } = new();
    }
}
