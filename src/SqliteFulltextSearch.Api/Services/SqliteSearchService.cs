// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using SqliteFulltextSearch.Api.Configuration;
using SqliteFulltextSearch.Api.Models;
using SqliteFulltextSearch.Shared.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SqliteFulltextSearch.Database;
using SqliteFulltextSearch.Database.Model;
using SqliteFulltextSearch.Shared.Constants;
using SqliteFulltextSearch.Shared.Models;
using System.Diagnostics;
using System.Text.Json;
using System.Globalization;

namespace SqliteFulltextSearch.Api.Services
{
    public class SqliteSearchService
    {
        private readonly ILogger<SqliteSearchService> _logger;

        private readonly ApplicationOptions _options;
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public SqliteSearchService(ILogger<SqliteSearchService> logger, IOptions<ApplicationOptions> options, IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _logger = logger;
            _options = options.Value;
            _dbContextFactory = dbContextFactory;
        }

        /// <summary>
        /// Searches the Database for a Document.
        /// </summary>
        /// <param name="query">Query Text</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>Search Results found for the given query</returns>
        public async Task<SearchResults> SearchAsync(string query, int from, int size, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            using var context = await _dbContextFactory
                .CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            // Build the raw SQLite FTS5 Query
            var sql = @"
                WITH documents_cte AS 
                (
                    SELECT f.rowid document_id, 
                        snippet(f.fts_document, 0, @highlightStartTag, @highlightEndTag, '', 32) match_title, 
                        snippet(f.fts_document, 1, @highlightStartTag, @highlightEndTag, '', 32) match_content
                    FROM 
                        fts_document f
                    WHERE 
                        f.fts_document MATCH @query 
                    ORDER BY f.rank
                ) 
                SELECT json_group_array(
                    json_object(
                        'document_id', document.document_id,
                        'title', document.title,
                        'filename', document.filename,
                        'row_version', document.row_version,
                        'last_edited_by', document.last_edited_by,
                        'valid_from', document.valid_from,
                        'valid_to', document.valid_to,
                        'keywords', (
                            SELECT json_group_array(json_object(
                                'keyword_id', k.keyword_id, 
                                'name', k.name, 
                                'row_version', k.row_version, 
                                'last_edited_by', k.last_edited_by, 
                                'valid_from', k.valid_from, 
                                'valid_to', k.valid_to))
                            FROM document_keyword dk
                                INNER JOIN keyword k on dk.keyword_id = k.keyword_id
                            WHERE 
                                dk.document_id = documents_cte.document_id
                         ),
                         'suggestions', (
                            SELECT json_group_array(json_object(
                                'suggestion_id', s.suggestion_id, 
                                'name', s.name, 
                                'row_version', s.row_version, 
                                'last_edited_by', s.last_edited_by, 
                                'valid_from', s.valid_from, 
                                'valid_to', s.valid_to))
                            FROM document_suggestion ds
                                INNER JOIN suggestion s on ds.suggestion_id = s.suggestion_id
                            WHERE 
                                ds.document_id = documents_cte.document_id
                         ),
                         'matches', json_object(
                            'title', documents_cte.match_title, 
                            'content', documents_cte.match_content)
                    )
                ) value
                FROM documents_cte
                    INNER JOIN document document ON documents_cte.document_id = document.document_id";

            // Escape the FTS Query
            var escapedFtsQuery = FtsEscape(query);

            // Set the Query as a Parameter to avoid SQL Injections
            var parameters = new[]
            {
                new SqliteParameter("@query", $"{{title content}}: {escapedFtsQuery}*"),
                new SqliteParameter("@highlightStartTag", SqliteConstants.Highlighter.HighlightStartTag),
                new SqliteParameter("@highlightEndTag", SqliteConstants.Highlighter.HighlightEndTag),
            };

            // Let's measure how long it took...
            var executionTimer = new Stopwatch();

            executionTimer.Start();

            var json = await context.Database
                .SqlQueryRaw<string>(sql, parameters)
                .FirstOrDefaultAsync();

            executionTimer.Stop();

            // So we know how long Sqlite took.
            var tookInMilliseconds = executionTimer.ElapsedMilliseconds;

            if (string.IsNullOrWhiteSpace(json))
            {
                return new SearchResults
                {
                    Query = query,
                    From = from,
                    Size = size,
                    Total = 0,
                    TookInMilliseconds = tookInMilliseconds,
                    Results = []
                };
            }

            var matches = JsonSerializer.Deserialize<List<SearchQueryResultDto>>(json);

            if (matches == null)
            {
                return new SearchResults
                {
                    Query = query,
                    From = from,
                    Size = size,
                    Total = 0,
                    TookInMilliseconds = tookInMilliseconds,
                    Results = []
                };
            }

            var total = matches.Count;

            // Paginate in LINQ
            var hits = matches
                .Skip(from)
                .Take(size)
                .ToList();

            var searchResults = ConvertToSearchResults(query, total, from, size, tookInMilliseconds, hits);

            return searchResults;
        }

        /// <summary>
        /// Searches the Database for Suggestions.
        /// </summary>
        /// <param name="query">Query Text</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>Suggestions found for the given query</returns>
        public async Task<SearchSuggestions> SuggestAsync(string query, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            using var context = await _dbContextFactory
                .CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            // Build the raw SQLite FTS5 Query
            var sql = @"
                WITH suggestions_cte AS 
                (
                    SELECT s.rowid suggestion_id, 
                        highlight(s.fts_suggestion, 0, @highlightStartTag, @highlightEndTag) match_suggestion
                    FROM 
                        fts_suggestion s
                    WHERE 
                        s.fts_suggestion MATCH @query
                    ORDER BY s.rank
                ) 
                SELECT json_group_array(
                    json_object(
                        'suggestion_id', suggestion.suggestion_id,
                        'name', suggestion.name,
                        'highlight', suggestions_cte.match_suggestion,
                        'row_version', suggestion.row_version,
                        'last_edited_by', suggestion.last_edited_by,
                        'valid_from', suggestion.valid_from,
                        'valid_to', suggestion.valid_to
                    )
                )  value
                FROM suggestions_cte
                    INNER JOIN suggestion ON suggestions_cte.suggestion_id = suggestion.suggestion_id";

            // Escape the Query
            var escapedFtsQuery = FtsEscape(query);

            // Set the Query as a Parameter to avoid SQL Injections
            var parameters = new[]
            {
                new SqliteParameter("@query", $"{{name}}: {escapedFtsQuery}*"),
                new SqliteParameter("@highlightStartTag", SqliteConstants.Highlighter.HighlightStartTag),
                new SqliteParameter("@highlightEndTag", SqliteConstants.Highlighter.HighlightEndTag),

            };

            // Let's measure how long it took...
            var executionTimer = new Stopwatch();

            executionTimer.Start();

            var json = await context.Database
                .SqlQueryRaw<string>(sql, parameters)
                .FirstOrDefaultAsync();

            executionTimer.Stop();

            // So we know how long Sqlite took.
            var tookInMilliseconds = executionTimer.ElapsedMilliseconds;

            if (string.IsNullOrWhiteSpace(json))
            {
                return new SearchSuggestions
                {
                    Query = query,
                    TookInMilliseconds = tookInMilliseconds,
                    Results = []
                };
            }

            var suggestions = JsonSerializer.Deserialize<List<SuggestQueryResultDto>>(json);

            if (suggestions == null)
            {
                return new SearchSuggestions
                {
                    Query = query,
                    TookInMilliseconds = tookInMilliseconds,
                    Results = []
                };
            }

            var searchSuggestions = ConvertToSearchSuggestions(query, tookInMilliseconds, suggestions);

            return searchSuggestions;
        }

        /// <summary>
        /// Converts from a <see cref="SearchResponse{TDocument}"> to <see cref="SearchSuggestions"/>.
        /// </summary>
        /// <param name="query">Query Text</param>
        /// <param name="searchResponse">Raw Search Response</param>
        /// <returns>Converted Search Suggestions</returns>
        private SearchSuggestions ConvertToSearchSuggestions(string query, long tookInMilliseconds, List<SuggestQueryResultDto> suggestions)
        {
            _logger.TraceMethodEntry();

            return new SearchSuggestions
            {
                Query = query,
                TookInMilliseconds = tookInMilliseconds,
                Results = GetSuggestions(suggestions)
            };
        }

        /// <summary>
        /// Gets the List of suggestions from a given <see cref="SearchResponse{TDocument}">.
        /// </summary>
        /// <param name="searchResponse">Raw Elasticsearch Search Response</param>
        /// <returns>Lust of Suggestions</returns>
        private List<SearchSuggestion> GetSuggestions(List<SuggestQueryResultDto> suggestions)
        {
            _logger.TraceMethodEntry();

            List<SearchSuggestion> result = [];

            foreach (var suggestion in suggestions)
            {
                result.Add(new SearchSuggestion
                {
                    Text = suggestion.Name,
                    Highlight = suggestion.Highlight
                });
            }

            return result;
        }

        /// <summary>
        /// Converts the <see cref="SearchQueryResultDto"/> to <see cref="SearchResults"/>.
        /// </summary>
        /// <param name="query">Original Query</param>
        /// <param name="searchResponse">Search Response from Elasticsearch</param>
        /// <returns>Search Results for a given Query</returns>
        private SearchResults ConvertToSearchResults(string query, int total, int from, int size, long tookInMilliseconds, List<SearchQueryResultDto> documents)
        {
            _logger.TraceMethodEntry();

            if (documents.Count == 0)
            {
                return new SearchResults
                {
                    Query = query,
                    From = from,
                    Size = size,
                    Total = 0,
                    TookInMilliseconds = tookInMilliseconds,
                    Results = []
                };
            }

            List<SearchResult> searchResults = [];

            foreach (var document in documents)
            {
                var searchResult = new SearchResult
                {
                    Identifier = document.Id.ToString(),
                    Title = document.Title,
                    Filename = document.Filename,
                    Keywords = document.Keywords
                        .Select(x => x.Name)
                        .ToList(),
                    Matches = [
                        document.Match.Content
                    ],
                    Url = $"{_options.BaseUri}/download/{document.Id}"
                };

                searchResults.Add(searchResult);
            }

            return new SearchResults
            {
                Query = query,
                Total = total,
                From = from,
                Size = size,
                TookInMilliseconds = tookInMilliseconds,
                Results = searchResults
            };
        }

        private string FtsEscape(string text)
        {
            var replaceQuotes = text.Replace("\"", "\"\"");

            return $"\"{replaceQuotes}\"";
        }
    }
}
