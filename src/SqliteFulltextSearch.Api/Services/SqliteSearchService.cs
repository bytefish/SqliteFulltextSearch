// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.IndexManagement;
using ElasticsearchFulltextExample.Api.Configuration;
using ElasticsearchFulltextExample.Api.Infrastructure.Exceptions;
using ElasticsearchFulltextExample.Api.Models;
using ElasticsearchFulltextExample.Shared.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SqliteFulltextSearch.Database;
using SqliteFulltextSearch.Database.Model;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace ElasticsearchFulltextExample.Api.Services
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

        public async Task DeleteAllAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();
        }

        public async Task IndexDocumentAsync(int documentId, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            using var context = await _dbContextFactory
                .CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            var document = await context.Documents
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == documentId, cancellationToken);

            if (document == null)
            {
                throw new EntityNotFoundException
                {
                    EntityName = nameof(Document),
                    EntityId = documentId,
                };
            }

            // Load the Keywords for the Document
            var keywords = await GetKeywordsByDocumentId(context, documentId, cancellationToken)
                .ConfigureAwait(false);

            // Load the Suggestions for the Document
            var suggestions = await GetSuggestionsByDocumentId(context, documentId, cancellationToken)
                .ConfigureAwait(false);

            // Index the document ...

            // Update the IndexedAt Timestamp
            await context.Documents
                .Where(x => x.Id == documentId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.IndexedAt, DateTime.UtcNow), cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Loads the Suggestions for a Document given a Document ID.
        /// </summary>
        /// <param name="context">DbContext to read from</param>
        /// <param name="documentId">Document ID</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>List of Suggestions associated with a Document</returns>
        private async Task<List<Suggestion>> GetSuggestionsByDocumentId(ApplicationDbContext context, int documentId, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            // Join Documents, DocumentSuggestions and Suggestions
            var suggestionQueryable = from document in context.Documents
                                      join documentSuggestion in context.DocumentSuggestions
                                          on document.Id equals documentSuggestion.DocumentId
                                      join suggestion in context.Suggestions
                                          on documentSuggestion.SuggestionId equals suggestion.Id
                                      where
                                        document.Id == documentId
                                      select suggestion;

            List<Suggestion> suggestions = await suggestionQueryable.AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return suggestions;
        }

        /// <summary>
        /// Loads the Keywords associated with a given Document.
        /// </summary>
        /// <param name="context">DbContext to use</param>
        /// <param name="documentId">Document ID</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>List of Keywords associated with a given Document</returns>
        private async Task<List<Keyword>> GetKeywordsByDocumentId(ApplicationDbContext context, int documentId, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            // Join Documents, DocumentKeywords and Keywords
            var keywordQueryable = from document in context.Documents
                                   join documentKeyword in context.DocumentKeywords
                                       on document.Id equals documentKeyword.DocumentId
                                   join keyword in context.Keywords
                                       on documentKeyword.KeywordId equals keyword.Id
                                   where
                                     document.Id == documentId
                                   select keyword;

            List<Keyword> keywords = await keywordQueryable.AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return keywords;
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
                SELECT d.document_id, d.title, d.filename, 
                    snippet(f.fts_document, 0, 'match→', '←match', '', 32) match_title, 
                    snippet(f.fts_document, 1, 'match→', '←match', '', 32) match_content
                FROM 
                    fts_document f
                        INNER JOIN document d on f.row_id = d.document_id
                WHERE 
                    fts_document MATCH '{title content}: OpenCV' 
                ORDER BY rank";

            // Set the Query as a Parameter to avoid SQL Injections
            var parameters = new[]
            {
                new SqliteParameter("@query", query),
                new SqliteParameter("@size", size),
                new SqliteParameter("@from", from),
            };

            // Let's measure how long it took...
            var executionTimer = new Stopwatch();

            executionTimer.Start();

            var matches = await context.Database
                .SqlQueryRaw<Document>(sql, parameters)
                .ToListAsync();

            executionTimer.Stop();

            // So we know how long Sqlite took.
            var tookInMilliseconds = executionTimer.ElapsedMilliseconds;

            var total = matches.Count;

            var hits = matches
                .Skip(from)
                .Take(size)
                .ToList();

            var searchResults = ConvertToSearchResults(query, from, size, tookInMilliseconds, hits);

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

            

            var searchSuggestions = ConvertToSearchSuggestions(query, suggestResponse);

            return searchSuggestions;
        }

        /// <summary>
        /// Deletes a Document by its Document ID.
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>The Delete Reponse from Elasticsearch</returns>
        public async Task<DeleteResponse> DeleteDocumentAsync(int documentId, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            return null;
        }

        /// <summary>
        /// Updates a Document by its Document ID.
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>The Delete Reponse from Elasticsearch</returns>
        public async Task<DeleteResponse> UpdateDocumentAsync(int documentId, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            return null;
        }

        /// <summary>
        /// Converts from a <see cref="SearchResponse{TDocument}"> to <see cref="SearchSuggestions"/>.
        /// </summary>
        /// <param name="query">Query Text</param>
        /// <param name="searchResponse">Raw Search Response</param>
        /// <returns>Converted Search Suggestions</returns>
        private SearchSuggestions ConvertToSearchSuggestions(string query, List<Suggestion> suggestions)
        {
            _logger.TraceMethodEntry();

            return new SearchSuggestions
            {
                Query = query,
                Results = GetSuggestions(suggestions)
            };
        }

        /// <summary>
        /// Gets the List of suggestions from a given <see cref="SearchResponse{TDocument}">.
        /// </summary>
        /// <param name="searchResponse">Raw Elasticsearch Search Response</param>
        /// <returns>Lust of Suggestions</returns>
        private List<SearchSuggestion> GetSuggestions(List<Suggestion> suggestions)
        {
            _logger.TraceMethodEntry();

            List<SearchSuggestion> result = [];

            foreach (var suggestion in suggestions)
            {
                var text = suggestion.Name;

                result.Add(new SearchSuggestion { Text = text, Highlight = text });
            }

            return result;
        }

        /// <summary>
        /// Replaces a string at a given index.
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="index">Start Index</param>
        /// <param name="length">Length</param>
        /// <param name="replace">Replacement Value</param>
        /// <returns></returns>
        public string ReplaceAt(string str, int index, int length, string replace)
        {
            _logger.TraceMethodEntry();

            return str
                .Remove(index, Math.Min(length, str.Length - index))
                .Insert(index, replace);
        }

        /// <summary>
        /// Converts a raw <see cref="SearchResponse{TDocument}"/> to <see cref="SearchResults"/>.
        /// </summary>
        /// <param name="query">Original Query</param>
        /// <param name="searchResponse">Search Response from Elasticsearch</param>
        /// <returns>Search Results for a given Query</returns>
        private SearchResults ConvertToSearchResults(string query, int total, int from, int size, long tookInMilliseconds, List<Document> documents)
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
                    Keywords = hit.Source.Keywords.ToList(),
                    Matches = GetMatches(hit.Highlight),
                    Url = $"{_options.BaseUri}/raw/{document.Id}"
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
    }
}
