
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
        /// Gets or sets the Match (Hidden FTS5 Column).
        /// </summary>
        public string? Match { get; set; }

        /// <summary>
        /// Gets or sets the Rank (Hidden FTS5 Column).
        /// </summary>
        public double? Rank { get; set; }

        /// <summary>
        /// Gets or sets the Document.
        /// </summary>
        public Document Document { get; set; } = null!;
    }
}
