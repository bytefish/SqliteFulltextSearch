// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace SqliteFulltextSearch.Api.Models
{
    /// <summary>
    /// Metadata for a Document, such as a PDF or Word Document.
    /// </summary>
    public class DocumentMetadata
    {
        /// <summary>
        /// Gets or sets the Author.
        /// </summary>
        public string? Author { get; set; }

        /// <summary>
        /// Gets or sets the Title.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the Subject.
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// Gets or sets the Content.
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Gets or sets the Creator.
        /// </summary>
        public string? Creator { get; set; }

        /// <summary>
        /// Gets or sets the Creation Date.
        /// </summary>
        public string? CreationDate { get; set; }
    }
}
