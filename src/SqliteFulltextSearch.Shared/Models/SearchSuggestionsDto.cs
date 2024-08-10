using System.Text.Json.Serialization;

namespace SqliteFulltextSearch.Shared.Models
{
    public class SearchSuggestionsDto
    {
        [JsonPropertyName("query")]
        public required string Query { get; set; }


        [JsonPropertyName("results")]
        public List<SearchSuggestionDto> Results { get; set; } = [];
    }
}
