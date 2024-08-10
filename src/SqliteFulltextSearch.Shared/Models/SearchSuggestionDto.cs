using System.Text.Json.Serialization;

namespace SqliteFulltextSearch.Shared.Models
{
    public class SearchSuggestionDto
    {
        [JsonPropertyName("text")]
        public required string Text { get; set; }


        [JsonPropertyName("highlight")]
        public required string Highlight { get; set; }
    }
}
