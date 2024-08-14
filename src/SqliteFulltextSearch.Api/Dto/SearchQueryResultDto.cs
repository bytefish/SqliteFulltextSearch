using SqliteFulltextSearch.Shared.Infrastructure;
using System.Text.Json.Serialization;

namespace SqliteFulltextSearch.Shared.Models
{
    public class SearchQueryResultDto
    {
        /// <summary>
        /// Gets or sets the DocumentId.
        /// </summary>
        [JsonPropertyName("document_id")]
        public required int DocumentId { get; set; }

        /// <summary>
        /// Gets or sets the Title.
        /// </summary>
        [JsonPropertyName("title")]
        public required string Title { get; set; }

        /// <summary>
        /// Gets or sets the Filename.
        /// </summary>
        [JsonPropertyName("filename")]
        public required string Filename { get; set; }

        /// <summary>
        /// Gets or sets the Keywords.
        /// </summary>
        [JsonPropertyName("keywords")]
        public List<string> Keywords { get; set; } = [];

        /// <summary>
        /// Gets or sets the Match for the Title.
        /// </summary>
        [JsonPropertyName("match_title")]
        public string? MatchTitle { get; set; }

        /// <summary>
        /// Gets or sets the Match for the Content.
        /// </summary>
        [JsonPropertyName("match_content")]
        public string? MatchContent { get; set; }
    }
}
