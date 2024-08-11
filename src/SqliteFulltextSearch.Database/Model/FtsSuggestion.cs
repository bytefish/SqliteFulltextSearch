
namespace SqliteFulltextSearch.Database.Model
{
    public class FtsSuggestion
    {
        /// <summary>
        /// Gets or sets the RowID.
        /// </summary>
        public int RowId { get; set; }

        /// <summary>
        /// Gets or sets the Suggestion.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the Document.
        /// </summary>
        public Suggestion Suggestion { get; set; } = null!;
    }
}
