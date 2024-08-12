using SqliteFulltextSearch.Shared.Infrastructure;
using System.Text.Json.Serialization;

namespace SqliteFulltextSearch.Shared.Models
{
    public class SearchQueryResultDto
    {
        public class MatchDto
        {
            [JsonPropertyName("title")]
            public string? Title { get; set; }

            [JsonPropertyName("content")]
            public string? Content { get; set; }
        }

        public class KeywordDto
        {
            [JsonPropertyName("keyword_id")]
            public int Id { get; set; }

            [JsonPropertyName("name")]
            public required string Name { get; set; }

            [JsonPropertyName("last_edited_by")]
            public int LastEditedBy { get; set; }

            [JsonPropertyName("row_version")]
            public int RowVersion { get; set; }

            [JsonPropertyName("valid_from")]
            [JsonConverter(typeof(SqliteDateTimeConverter))]
            public DateTime ValidFrom { get; set; }

            [JsonPropertyName("valid_to")]
            [JsonConverter(typeof(SqliteDateTimeConverter))]
            public DateTime ValidTo { get; set; }

        }

        public class SuggestionDto
        {
            [JsonPropertyName("suggestion_id")]
            public int Id { get; set; }

            [JsonPropertyName("name")]
            public required string Name { get; set; }

            [JsonPropertyName("last_edited_by")]
            public int LastEditedBy { get; set; }

            [JsonPropertyName("row_version")]
            public int RowVersion { get; set; }

            [JsonPropertyName("valid_from")]
            [JsonConverter(typeof(SqliteDateTimeConverter))]
            public DateTime ValidFrom { get; set; }

            [JsonPropertyName("valid_to")]
            [JsonConverter(typeof(SqliteDateTimeConverter))]
            public DateTime ValidTo { get; set; }
        }

        [JsonPropertyName("document_id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public required string Title { get; set; }

        [JsonPropertyName("filename")]
        public required string Filename { get; set; }

        [JsonPropertyName("last_edited_by")]
        public int LastEditedBy { get; set; }

        [JsonPropertyName("row_version")]
        public int RowVersion { get; set; }

        [JsonPropertyName("valid_from")]
        [JsonConverter(typeof(SqliteDateTimeConverter))]
        public DateTime ValidFrom { get; set; }

        [JsonPropertyName("valid_to")]
        [JsonConverter(typeof(SqliteDateTimeConverter))]
        public DateTime ValidTo { get; set; }

        [JsonPropertyName("keywords")]
        public List<KeywordDto> Keywords { get; set; } = [];

        [JsonPropertyName("suggestions")]
        public List<SuggestionDto> Suggestions { get; set; } = [];

        [JsonPropertyName("matches")]
        public required MatchDto Match { get; set; }
    }
}
