// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace SqliteFulltextSearch.Database.Model
{
    public abstract class Entity
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the user the entity row version.
        /// </summary>
        public uint? RowVersion { get; set; }

        /// <summary>
        /// Gets or sets the user, that made the latest modifications.
        /// </summary>
        public required int LastEditedBy { get; set; }

        /// <summary>
        /// Gets or sets the SysPeriod.
        /// </summary>
        public DateTime? ValidFrom { get; set; }

        /// <summary>
        /// Gets or sets the SysPeriod.
        /// </summary>
        public DateTime? ValidTo { get; set; }
    }
}