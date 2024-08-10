using System.Text.Json.Serialization;

namespace SqliteFulltextSearch.Shared.Models
{
    public class DocumentSearchDto
    {
        [JsonPropertyName("document_id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public required string Title { get; set; }

        [JsonPropertyName("filename")]
        public required string Filename { get; set; }

        [JsonPropertyName("keywords")]
        public List<KeywordDto> Keywords { get; set; } = [];

        [JsonPropertyName("suggestions")]
        public List<SuggestionDto> Suggestions { get; set; } = [];

        [JsonPropertyName("matches")]
        public required MatchDto Match { get; set; }
    }
}
