// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace ElasticsearchFulltextExample.Shared.Models
{
    public class SearchResultDto
    {
        [JsonPropertyName("identifier")]
        public required string Identifier { get; set; }

        [JsonPropertyName("title")]
        public required string Title { get; set; }

        [JsonPropertyName("filename")]
        public required string Filename { get; set; }

        [JsonPropertyName("matches")]
        public List<string> Matches { get; set; } = new();

        [JsonPropertyName("keywords")]
        public List<string> Keywords { get; set; } = new();

        [JsonPropertyName("url")]
        public required string? Url { get; set; }
    }
}
