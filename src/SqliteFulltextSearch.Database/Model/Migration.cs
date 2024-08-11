namespace SqliteFulltextSearch.Database.Model
{
    public class Migration : Entity
    {
        /// <summary>
        /// Gets or sets the Version.
        /// </summary>
        public required string Version { get; set; }

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        public required string Description { get; set; }
    }
}
