// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace ElasticsearchFulltextExample.Shared.Models
{
    public class SearchResultsDto
    {
        [JsonPropertyName("query")]
        public required string Query { get; set; }

        [JsonPropertyName("from")]
        public required int From { get; set; }

        [JsonPropertyName("size")]
        public required int Size { get; set; }

        [JsonPropertyName("tookInMilliseconds")]
        public required long TookInMilliseconds { get; set; }

        [JsonPropertyName("total")]
        public required long Total { get; set; }

        [JsonPropertyName("results")]
        public List<SearchResultDto> Results { get; set; } = [];

    }
}
