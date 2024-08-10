using System.Text.Json.Serialization;

namespace SqliteFulltextSearch.Shared.Models
{
    public class MatchDto
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}
