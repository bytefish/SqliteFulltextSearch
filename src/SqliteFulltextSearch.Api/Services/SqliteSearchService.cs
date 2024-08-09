// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.IndexManagement;
using ElasticsearchFulltextExample.Api.Configuration;
using ElasticsearchFulltextExample.Api.Infrastructure.Elasticsearch;
using ElasticsearchFulltextExample.Api.Infrastructure.Elasticsearch.Models;
using ElasticsearchFulltextExample.Api.Infrastructure.Exceptions;
using ElasticsearchFulltextExample.Api.Models;
using ElasticsearchFulltextExample.Database;
using ElasticsearchFulltextExample.Database.Model;
using ElasticsearchFulltextExample.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace ElasticsearchFulltextExample.Api.Services
{
    public class ElasticsearchService
    {
        private readonly ILogger<ElasticsearchService> _logger;

        private readonly ApplicationOptions _options;
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly ElasticsearchSearchClient _elasticsearchSearchClient;

        public ElasticsearchService(ILogger<ElasticsearchService> logger, IOptions<ApplicationOptions> options, IDbContextFactory<ApplicationDbContext> dbContextFactory, ElasticsearchSearchClient elasticsearchClient)
        {
            _logger = logger;
            _options = options.Value;
            _dbContextFactory = dbContextFactory;
            _elasticsearchSearchClient = elasticsearchClient;
        }

        public async Task CreateIndexAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            await _elasticsearchSearchClient
                .CreateIndexAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task CreatePipelineAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            await _elasticsearchSearchClient
                .CreatePipelineAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task DeleteAllAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            await _elasticsearchSearchClient.DeleteAllAsync(cancellationToken);
        }

        public async Task DeleteIndexAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            await _elasticsearchSearchClient.DeleteIndexAsync(cancellationToken);
        }

        public async Task DeletePipelineAsync(string pipeline, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            await _elasticsearchSearchClient.DeletePipelineAsync(pipeline, cancellationToken);
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

            // Build the Document to send to Elasticsearch
            var elasticsearchDocument = new ElasticsearchDocument
            {
                Id = document.Id.ToString(),
                Title = document.Title,
                Filename = document.Filename,
                Suggestions = suggestions
                    .Select(x => x.Name)
                    .ToArray(),
                Keywords = keywords
                    .Select(x => x.Name)
                    .ToArray(),
                Data = document.Data,
                IndexedOn = null, // Will be set after indexing ...
                Attachment = null // Will be set after indexing ...
            };

            // Send to Elasticsearch for indexing
            await _elasticsearchSearchClient.IndexAsync(elasticsearchDocument, cancellationToken);

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

            var searchResponse = await _elasticsearchSearchClient
                .SearchAsync(query, from, size, cancellationToken)
                .ConfigureAwait(false);

            var searchResults = ConvertToSearchResults(query, from, size, searchResponse);

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

            var suggestResponse = await _elasticsearchSearchClient
                .SuggestAsync(query, cancellationToken)
                .ConfigureAwait(false);

            var searchSuggestions = ConvertToSearchSuggestions(query, suggestResponse);

            return searchSuggestions;
        }

        public async Task<List<SearchStatistics>> GetSearchStatisticsAsync(CancellationToken cancellationToken)
        {
            var indicesStatsResponse = await _elasticsearchSearchClient
                .GetSearchStatistics(cancellationToken)
                .ConfigureAwait(false);

            var searchStatistics = ConvertToSearchStatistics(indicesStatsResponse);

            return searchStatistics;
        }

        public static List<SearchStatistics> ConvertToSearchStatistics(IndicesStatsResponse indicesStatsResponse)
        {
            if (indicesStatsResponse.Indices == null)
            {
                throw new Exception("No statistics available");
            }

            return indicesStatsResponse.Indices
                .Select(x => ConvertToSearchStatistics(x.Key, x.Value))
                .ToList();
        }

        public static SearchStatistics ConvertToSearchStatistics(string indexName, IndicesStats indexStats)
        {
            return new SearchStatistics
            {
                IndexName = indexName,
                IndexSizeInBytes = indexStats.Total?.Store?.SizeInBytes,
                TotalNumberOfDocumentsIndexed = indexStats.Total?.Docs?.Count,
                NumberOfDocumentsCurrentlyBeingIndexed = indexStats.Total?.Indexing?.IndexCurrent,
                TotalNumberOfFetches = indexStats.Total?.Search?.FetchTotal,
                NumberOfFetchesCurrentlyInProgress = indexStats.Total?.Search?.FetchCurrent,
                TotalNumberOfQueries = indexStats.Total?.Search?.QueryTotal,
                NumberOfQueriesCurrentlyInProgress = indexStats.Total?.Search?.QueryCurrent,
                TotalTimeSpentBulkIndexingDocumentsInMilliseconds = indexStats.Total?.Bulk?.TotalTimeInMillis,
                TotalTimeSpentIndexingDocumentsInMilliseconds = indexStats.Total?.Bulk?.TotalTimeInMillis,
                TotalTimeSpentOnFetchesInMilliseconds = indexStats.Total?.Search?.FetchTimeInMillis,
                TotalTimeSpentOnQueriesInMilliseconds = indexStats.Total?.Search?.QueryTimeInMillis,
            };
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

            return await _elasticsearchSearchClient
                .DeleteAsync(documentId.ToString(CultureInfo.InvariantCulture), cancellationToken);
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

            return await _elasticsearchSearchClient
                .DeleteAsync(documentId.ToString(CultureInfo.InvariantCulture), cancellationToken);
        }

        /// <summary>
        /// Pings the Elasticsearch Cluster.
        /// </summary>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>Ping Response from Elasticsearch</returns>
        public async Task<PingResponse> PingAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            return await _elasticsearchSearchClient.PingAsync(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Converts from a <see cref="SearchResponse{TDocument}"> to <see cref="SearchSuggestions"/>.
        /// </summary>
        /// <param name="query">Query Text</param>
        /// <param name="searchResponse">Raw Search Response</param>
        /// <returns>Converted Search Suggestions</returns>
        private SearchSuggestions ConvertToSearchSuggestions(string query, SearchResponse<ElasticsearchDocument> searchResponse)
        {
            _logger.TraceMethodEntry();

            return new SearchSuggestions
            {
                Query = query,
                Results = GetSuggestions(searchResponse)
            };
        }

        /// <summary>
        /// Gets the List of suggestions from a given <see cref="SearchResponse{TDocument}">.
        /// </summary>
        /// <param name="searchResponse">Raw Elasticsearch Search Response</param>
        /// <returns>Lust of Suggestions</returns>
        private List<SearchSuggestion> GetSuggestions(SearchResponse<ElasticsearchDocument> searchResponse)
        {
            _logger.TraceMethodEntry();

            if (searchResponse == null)
            {
                return [];
            }

            var suggest = searchResponse.Suggest;

            if (suggest == null)
            {
                return [];
            }

            if (!suggest.ContainsKey("suggest"))
            {
                return [];
            }

            var suggestions = suggest["suggest"];

            if (suggestions == null)
            {
                return [];
            }

            var result = new List<SearchSuggestion>();

            foreach (var suggestion in suggestions)
            {
                var completionSuggest = suggestion as CompletionSuggest<ElasticsearchDocument>;

                // This is not a Completion Suggest...
                if (completionSuggest == null)
                {
                    if (_logger.IsInformationEnabled())
                    {
                        _logger.LogInformation("Suggestion {Suggestion} is no Completion Suggestion. Ignored.", suggest);
                    }

                    continue;
                }

                var offset = completionSuggest.Offset;
                var length = completionSuggest.Length;

                foreach (var option in completionSuggest.Options)
                {
                    var text = option.Text;
                    var prefix = option.Text.Substring(offset, Math.Min(length, text.Length));
                    var highlight = ReplaceAt(option.Text, offset, length, $"<strong>{prefix}</strong>");

                    result.Add(new SearchSuggestion { Text = text, Highlight = highlight });
                }
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
        private SearchResults ConvertToSearchResults(string query, int from, int size, SearchResponse<ElasticsearchDocument> searchResponse)
        {
            _logger.TraceMethodEntry();

            var hits = searchResponse.Hits;

            if (hits.Count == 0)
            {
                return new SearchResults
                {
                    Query = query,
                    From = from,
                    Size = size,
                    Total = 0,
                    TookInMilliseconds = searchResponse.Took,
                    Results = []
                };
            }

            List<SearchResult> searchResults = [];

            foreach (var hit in hits)
            {
                if (hit.Source == null)
                {
                    if (_logger.IsWarningEnabled())
                    {
                        _logger.LogWarning("Got a hit, but it has no source (Query = '{Query}', Hit = '{Hit}')", query, hit);
                    }

                    continue;
                }

                var searchResult = new SearchResult
                {
                    Identifier = hit.Source.Id,
                    Title = hit.Source.Title,
                    Filename = hit.Source.Filename,
                    Keywords = hit.Source.Keywords.ToList(),
                    Matches = GetMatches(hit.Highlight),
                    Url = $"{_options.BaseUri}/raw/{hit.Source.Id}"
                };

                searchResults.Add(searchResult);
            }

            return new SearchResults
            {
                Query = query,
                From = from,
                Size = size,
                Total = searchResponse.Total,
                TookInMilliseconds = searchResponse.Took,
                Results = searchResults
            };
        }

        private List<string> GetMatches(IReadOnlyDictionary<string, IReadOnlyCollection<string>>? highlight)
        {
            _logger.TraceMethodEntry();

            if (highlight == null)
            {
                return [];
            }

            return GetMatchesForField(highlight, "attachment.content");
        }

        private List<string> GetMatchesForField(IReadOnlyDictionary<string, IReadOnlyCollection<string>> highlight, string field)
        {
            _logger.TraceMethodEntry();

            if (highlight == null)
            {
                return [];
            }

            if (!highlight.TryGetValue(field, out var matches))
            {
                return [];
            }

            return matches
                .ToList();
        }
    }
}
