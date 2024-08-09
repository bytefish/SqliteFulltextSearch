// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Api.Services;
using ElasticsearchFulltextExample.Api.Models;
using ElasticsearchFulltextExample.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ElasticsearchFulltextExample.Api.Infrastructure.Errors;
using Elastic.Clients.Elasticsearch;

namespace ElasticsearchFulltextExample.Api
{
    public static class SearchApi
    {
        private const string Tags = "search";

        public static IEndpointRouteBuilder MapSearchEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/delete-index",
                async (SqliteSearchService elasticsearchService, CancellationToken cancellationToken) =>
                {
                    await elasticsearchService
                        .DeleteIndexAsync(cancellationToken)
                        .ConfigureAwait(false);

                    return Results.Ok();
                })
                .AddEndpointFilter<ApplicationErrorExceptionFilter>();

            app.MapPost("/delete-all-documents", async (SqliteSearchService elasticsearchService, CancellationToken cancellationToken) =>
            {
                await elasticsearchService
                    .DeleteAllAsync(cancellationToken)
                    .ConfigureAwait(false);

                return TypedResults.Ok();
            });

            app.MapPost("/create-index", async (SqliteSearchService elasticsearchService, CancellationToken cancellationToken) =>
            {
                await elasticsearchService
                    .CreateIndexAsync(cancellationToken)
                    .ConfigureAwait(false);

                return TypedResults.Ok();
            })
            .WithName("CreateIndex");

            app.MapPost("/create-pipeline", async (SqliteSearchService elasticsearchService, CancellationToken cancellationToken) =>
            {
                await elasticsearchService
                    .CreatePipelineAsync(cancellationToken)
                    .ConfigureAwait(false);

                return TypedResults.Ok();
            })
            .WithName("CreatePipeline");

            app.MapPost("/delete-pipeline/{pipeline}", async (SqliteSearchService elasticsearchService, [FromRoute(Name = "pipeline")] string pipeline, CancellationToken cancellationToken) =>
            {
                await elasticsearchService
                    .DeletePipelineAsync(pipeline, cancellationToken)
                    .ConfigureAwait(false);

                return TypedResults.Ok();
            })
            .WithName("DeletePipeline");


            app
                .MapGet("/statistics", GetSearchStatisticsAsync)
                .WithName("GetStatistics")

            app.MapGet("/suggest", )
            .WithName("Suggestions");

            app.MapPost("/upload", async (
                DocumentService documentService,
                [FromForm(Name = "title")] string title,
                [FromForm(Name = "suggestions")] List<string>? suggestions,
                [FromForm(Name = "keywords")] List<string>? keywords,
                [FromForm(Name = "data")] IFormFile data,
                CancellationToken cancellationToken) =>
            {
                var fileBytes = await GetBytesAsync(data).ConfigureAwait(false);

                await documentService
                    .CreateDocumentAsync(title, data.FileName, fileBytes, suggestions, keywords, Constants.Users.DataConversionUserId, cancellationToken)
                    .ConfigureAwait(false);

                return TypedResults.Created();
            })
            .WithName("PostDocument")
            .WithTags(Tags)
            .WithOpenApi();

            return app;
        }

        public static async Task<IResult> GetSearchStatisticsAsync(SqliteSearchService elasticsearchService, [FromQuery(Name = "q")] string query, CancellationToken cancellationToken)
        {
            var suggestResults = await elasticsearchService
                .SuggestAsync(query, cancellationToken)
                .ConfigureAwait(false);

            var suggestResultsDto = Convert(suggestResults);

            return TypedResults.Ok(suggestResultsDto);

        }

        public static async Task<IResult> GetSearchStatisticsAsync(SqliteSearchService elasticsearchService, CancellationToken cancellationToken)
        {
            var searchStatistics = await elasticsearchService.GetSearchStatisticsAsync(cancellationToken);

            var searchStatisticsDto = Convert(searchStatistics);

            return Results.Ok(searchStatisticsDto);
        }

        static List<SearchStatisticsDto> Convert(List<SearchStatistics> source)
        {
            return source
                .Select(x => Convert(x))
                .ToList();
        }

        static SearchStatisticsDto Convert(SearchStatistics source)
        {
            return new SearchStatisticsDto
            {
                IndexName = source.IndexName,
                IndexSizeInBytes = source.IndexSizeInBytes,
                NumberOfDocumentsCurrentlyBeingIndexed = source.NumberOfDocumentsCurrentlyBeingIndexed,
                NumberOfFetchesCurrentlyInProgress = source.NumberOfFetchesCurrentlyInProgress,
                NumberOfQueriesCurrentlyInProgress = source.NumberOfQueriesCurrentlyInProgress,
                TotalNumberOfDocumentsIndexed = source.TotalNumberOfDocumentsIndexed,
                TotalNumberOfQueries = source.TotalNumberOfQueries,
                TotalNumberOfFetches = source.TotalNumberOfFetches,
                TotalTimeSpentBulkIndexingDocumentsInMilliseconds = source.TotalTimeSpentBulkIndexingDocumentsInMilliseconds,
                TotalTimeSpentIndexingDocumentsInMilliseconds = source.TotalTimeSpentIndexingDocumentsInMilliseconds,
                TotalTimeSpentOnFetchesInMilliseconds = source.TotalTimeSpentOnFetchesInMilliseconds,
                TotalTimeSpentOnQueriesInMilliseconds = source.TotalTimeSpentOnQueriesInMilliseconds
            };
        }

        #region Converters

        static SearchResultsDto Convert(SearchResults source)
        {
            return new SearchResultsDto
            {
                Query = source.Query,
                From = source.From,
                Size = source.Size,
                TookInMilliseconds = source.TookInMilliseconds,
                Total = source.Total,
                Results = Convert(source.Results)
            };
        }

        static List<SearchResultDto> Convert(List<SearchResult> source)
        {
            return source
                .Select(searchResult => Convert(searchResult))
                .ToList();
        }

        static SearchResultDto Convert(SearchResult source)
        {
            return new SearchResultDto
            {
                Identifier = source.Identifier,
                Title = source.Title,
                Filename = source.Filename,
                Keywords = source.Keywords,
                Matches = source.Matches,
                Url = source.Url
            };
        }

        static SearchSuggestionsDto Convert(SearchSuggestions source)
        {
            return new SearchSuggestionsDto
            {
                Query = source.Query,
                Results = Convert(source.Results)
            };
        }

        static List<SearchSuggestionDto> Convert(List<SearchSuggestion> source)
        {
            return source
                .Select(suggestion => Convert(suggestion))
                .ToList();
        }

        static SearchSuggestionDto Convert(SearchSuggestion source)
        {
            return new SearchSuggestionDto
            {
                Text = source.Text,
                Highlight = source.Highlight
            };
        }

        static async Task<byte[]> GetBytesAsync(IFormFile formFile)
        {
            using (var memoryStream = new MemoryStream())
            {
                await formFile.CopyToAsync(memoryStream);

                return memoryStream.ToArray();
            }
        }

        #endregion
    }
}
