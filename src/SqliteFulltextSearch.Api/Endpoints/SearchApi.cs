// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using SqliteFulltextSearch.Api.Services;
using SqliteFulltextSearch.Api.Models;
using SqliteFulltextSearch.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using SqliteFulltextSearch.Api.Infrastructure.Errors;
using SqliteFulltextSearch.Shared.Constants;

namespace SqliteFulltextSearch.Api.Endpoints
{
    public static class SearchApi
    {
        private const string Tags = "search";

        public static IEndpointRouteBuilder MapSearchEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/delete-all-documents", DeleteAllAsync)
                .WithName("DeleteAllDocuments")
                .WithTags(Tags)
                .WithOpenApi()
                .AddEndpointFilter<ApplicationErrorExceptionFilter>();


            app.MapPost("/upload", UploadAsync)
                .WithName("PostDocument")
                .WithTags(Tags)
                .WithOpenApi()
                .AddEndpointFilter<ApplicationErrorExceptionFilter>()
                .DisableAntiforgery();

            app.MapGet("/suggest", SuggestAsync)
                .WithName("GetSuggestions")
                .WithTags(Tags)
                .WithOpenApi()
                .AddEndpointFilter<ApplicationErrorExceptionFilter>();


            app.MapGet("/search", SearchAsync)
                .WithName("GetSearchResults")
                .WithTags(Tags)
                .WithOpenApi()
                .AddEndpointFilter<ApplicationErrorExceptionFilter>();

            return app;
        }

        public static async Task<IResult> DeleteAllAsync(DocumentService documentService, CancellationToken cancellationToken)
        {
            await documentService
                .DeleteAllDocumentsAsync(cancellationToken)
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
            SqliteSearchService sqliteSearchService,
            IFormCollection form,
            CancellationToken cancellationToken)
        {
            (string Title, List<string> Suggestions, List<string> Keywords, IFormFile? Data) upload = (
                GetAsString(form, FileUploadNames.Title),
                GetAsList(form, FileUploadNames.Suggestions),
                GetAsList(form, FileUploadNames.Keywords),
                form.Files[FileUploadNames.Data]);

            // Add some better validation here ...
            if(upload.Data == null)
            {
                return TypedResults.BadRequest("Invalid Data");
            }

            var fileBytes = await GetBytesAsync(upload.Data)
                .ConfigureAwait(false);

            var document = await documentService
                .CreateDocumentAsync(upload.Title, upload.Data.FileName, fileBytes, upload.Suggestions, upload.Keywords, Constants.Users.DataConversionUserId, cancellationToken)
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

        #region Helper Methods

        private static async Task<byte[]> GetBytesAsync(IFormFile formFile)
        {
            using (var memoryStream = new MemoryStream())
            {
                await formFile.CopyToAsync(memoryStream);

                return memoryStream.ToArray();
            }
        }

        private static string GetAsString(IFormCollection form, string parameterName)
        {
            return form[parameterName].ToString();
        }

        private static List<string> GetAsList(IFormCollection form, string parameterName)
        {
            var orderedKeys = form.Keys
                .Where(x => x.StartsWith(parameterName))
                .Order();

            var result = orderedKeys
                .Select(x => form[x].ToString())
                .ToList();

            return result;
        }

        #endregion
    }
}