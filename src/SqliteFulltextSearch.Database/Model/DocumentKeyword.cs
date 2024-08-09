// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchFulltextExample.Database.Model
{
    /// <summary>
    /// Association between a Document and a Keyword.
    /// </summary>
    public class DocumentKeyword : Entity 
    {
        /// <summary>
        /// Gets or sets the DocumentId.
        /// </summary>
        public required int DocumentId { get; set; }
        
        /// <summary>
        /// Gets or sets the KeywordId.
        /// </summary>
        public required int KeywordId { get; set; }
    }
}