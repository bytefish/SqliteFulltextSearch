﻿// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace SqliteFulltextSearch.Database.Model
{
    public class Document : Entity
    {
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Gets or sets the original filename.
        /// </summary>
        public required string Filename { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        public required byte[] Data { get; set; }
    }
}
