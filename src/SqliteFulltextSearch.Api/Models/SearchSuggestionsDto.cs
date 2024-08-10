using System.Text.Json.Serialization;

namespace SqliteFulltextSearch.Api.Models
{
    public class SearchSuggestions
    {
        public required string Query { get; set; }

        public long TookInMilliseconds { get; set; } = 0;

        public List<SearchSuggestion> Results { get; set; } = new();
    }
}
