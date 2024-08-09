// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchFulltextExample.Database.Model
{
    /// <summary>
    /// A Suggestion.
    /// </summary>
    public class Suggestion : Entity
    {
        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        public required string Name { get; set; }
    }
}
