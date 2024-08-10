using System.Text.Json.Serialization;

namespace SqliteFulltextSearch.Shared.Models
{
    public class KeywordDto
    {
        [JsonPropertyName("keyword_id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("row_version")]
        public int RowVersion { get; set; }

        [JsonPropertyName("valid_from")]
        public DateTime ValidFrom { get; set; }

        [JsonPropertyName("valid_to")]
        public DateTime ValidTo { get; set; }
    }
}
