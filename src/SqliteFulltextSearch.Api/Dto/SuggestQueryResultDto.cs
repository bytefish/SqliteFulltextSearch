using SqliteFulltextSearch.Shared.Infrastructure;
using System.Text.Json.Serialization;

namespace SqliteFulltextSearch.Shared.Models
{
    public class SuggestQueryResultDto
    {
        [JsonPropertyName("suggestion_id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("highlight")]
        public required string Highlight { get; set; }

        [JsonPropertyName("row_version")]
        public int RowVersion { get; set; }

        [JsonPropertyName("valid_from")]
        [JsonConverter(typeof(SqliteDateTimeConverter))]
        public DateTime ValidFrom { get; set; }

        [JsonPropertyName("valid_to")]
        [JsonConverter(typeof(SqliteDateTimeConverter))]
        public DateTime ValidTo { get; set; }
    }
}
