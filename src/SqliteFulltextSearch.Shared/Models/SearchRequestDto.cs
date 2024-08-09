// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace ElasticsearchFulltextExample.Shared.Models
{
    public class SearchRequestDto
    {
        [JsonPropertyName("query")]
        public required string Query { get; set; }

        [JsonPropertyName("from")]
        public required int From { get; set; }

        [JsonPropertyName("to")]
        public required int To { get; set; }
    }
}
