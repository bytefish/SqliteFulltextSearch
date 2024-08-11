
namespace SqliteFulltextSearch.Database.Model
{
    public class FtsDocument
    {
        /// <summary>
        /// Gets or sets the RowID.
        /// </summary>
        public int RowId { get; set; }

        /// <summary>
        /// Gets or sets the Title.
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Gets or sets the Content.
        /// </summary>
        public required string Content { get; set; }

        /// <summary>
        /// Gets or sets the Document.
        /// </summary>
        public Document? Document { get; set; } = null!;
    }
}
