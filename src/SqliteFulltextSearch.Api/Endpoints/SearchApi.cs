// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using SqliteFulltextSearch.Api.Services;
using SqliteFulltextSearch.Api.Models;
using SqliteFulltextSearch.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using SqliteFulltextSearch.Api.Infrastructure.Errors;

namespace SqliteFulltextSearch.Api.Endpoints
{
    public static class SearchApi
    {
        private const string Tags = "search";

        public static IEndpointRouteBuilder MapSearchEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/delete-all-documents", DeleteAllAsync)
                .WithName("DeleteAllDocuments")
                .AddEndpointFilter<ApplicationErrorExceptionFilter>();


            app.MapPost("/upload", UploadAsync)
                .WithName("PostDocument")
                .AddEndpointFilter<ApplicationErrorExceptionFilter>();

            app.MapGet("/suggest", SuggestAsync)
                 .WithName("GetSuggestions")
                 .AddEndpointFilter<ApplicationErrorExceptionFilter>();


            app.MapGet("/search", SearchAsync)
                 .WithName("GetSearchResults")
                 .AddEndpointFilter<ApplicationErrorExceptionFilter>();

            return app;
        }

        #region Converters

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


        #endregion

        public static async Task<IResult> DeleteAllAsync(SqliteSearchService sqliteSearchService, CancellationToken cancellationToken)
        {
            await sqliteSearchService
                .DeleteAllAsync(cancellationToken)
                .ConfigureAwait(false);

            return TypedResults.Ok();
        }

        public static async Task<IResult> SearchAsync(SqliteSearchService sqliteSearchService,
            [FromQuery(Name = "q")] string query,
            [FromQuery(Name = "from")] int from,
            [FromQuery(Name = "size")] int size,
            CancellationToken cancellationToken)
        {
            var searchResults = await sqliteSearchService
                .SearchAsync(query, from, size, cancellationToken)
                .ConfigureAwait(false);

            var searchResultsDto = Convert(searchResults);

            return TypedResults.Ok(searchResultsDto);
        }

        public static async Task<IResult> SuggestAsync(SqliteSearchService sqliteSearchService, [FromQuery(Name = "q")] string query, CancellationToken cancellationToken)
        {
            var searchSuggestions = await sqliteSearchService
                .SuggestAsync(query, cancellationToken)
                .ConfigureAwait(false);

            var searchSuggestionsDto = Convert(searchSuggestions);

            return TypedResults.Ok(searchSuggestionsDto);
        }

        public static async Task<IResult> UploadAsync(DocumentService documentService,
                [FromForm(Name = "title")] string title,
                [FromForm(Name = "suggestions")] List<string>? suggestions,
                [FromForm(Name = "keywords")] List<string>? keywords,
                [FromForm(Name = "data")] IFormFile data,
                CancellationToken cancellationToken)
        {
            var fileBytes = await GetBytesAsync(data)
                .ConfigureAwait(false);

            await documentService
                .CreateDocumentAsync(title, data.FileName, fileBytes, suggestions, keywords, Constants.Users.DataConversionUserId, cancellationToken)
                .ConfigureAwait(false);

            return TypedResults.Created();
        }

        public static async Task<IResult> QueryAsync(SqliteSearchService sqliteSearchService,
            [FromQuery(Name = "q")] string query,
            [FromQuery(Name = "from")] int from,
            [FromQuery(Name = "size")] int size,
            CancellationToken cancellationToken)
        {
            var searchResults = await sqliteSearchService
                .SearchAsync(query, from, size, cancellationToken)
                .ConfigureAwait(false);

            var searchResultsDto = Convert(searchResults);

            return TypedResults.Ok(searchResultsDto);
        }
    }
}