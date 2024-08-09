// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchFulltextExample.Database.Model
{
    /// <summary>
    /// A Keyword.
    /// </summary>
    public class Keyword : Entity
    {
        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        public required string Name { get; set; }
    }
}
