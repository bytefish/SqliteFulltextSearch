using SqliteFulltextSearch.Shared.Infrastructure;
using System.Text.Json.Serialization;

namespace SqliteFulltextSearch.Shared.Models
{
    public class SuggestQueryResultDto
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("highlight")]
        public required string Highlight { get; set; }
    }
}
