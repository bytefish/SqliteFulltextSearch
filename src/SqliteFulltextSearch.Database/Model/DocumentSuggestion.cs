// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchFulltextExample.Database.Model
{
    /// <summary>
    /// Association between a Document and a Keyword.
    /// </summary>
    public class DocumentSuggestion : Entity
    {
        /// <summary>
        /// Gets or sets the DocumentId.
        /// </summary>
        public required int DocumentId { get; set; }
        
        /// <summary>
        /// Gets or sets the SuggestionId.
        /// </summary>
        public required int SuggestionId { get; set; }
    }
}