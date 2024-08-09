// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchFulltextExample.Api.Configuration
{
    public class ElasticsearchOptions
    {
        public required string Uri { get; set; }

        public required string IndexName { get; set; }
    }
}
