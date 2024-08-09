// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchFulltextExample.Api.Models
{
    public class SearchResult
    {
        public required string Identifier { get; set; }

        public required string Title { get; set; }
        
        public required string Filename { get; set; }

        public List<string> Matches { get; set; } = new();

        public List<string> Keywords { get; set; } = new();

        public string? Url { get; set; }
    }
}
