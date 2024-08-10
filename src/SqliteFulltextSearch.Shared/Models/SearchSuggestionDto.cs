﻿using System.Text.Json.Serialization;

namespace ElasticsearchFulltextExample.Shared.Models
{
    public class SearchSuggestionDto
    {
        [JsonPropertyName("suggestion_id")]
        public required int Id{ get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("highlight")]
        public required string Highlight { get; set; }

        [JsonPropertyName("last_edited_by")]
        public int LastEditedBy { get; set; }

        [JsonPropertyName("row_version")]
        public int RowVersion { get; set; }

        [JsonPropertyName("valid_from")]
        public DateTime ValidFrom { get; set; }

        [JsonPropertyName("valid_to")]
        public DateTime ValidTo { get; set; }

    }
}
