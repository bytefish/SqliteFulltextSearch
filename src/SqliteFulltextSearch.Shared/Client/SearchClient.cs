// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using SqliteFulltextSearch.Shared.Constants;
using SqliteFulltextSearch.Shared.Infrastructure;
using SqliteFulltextSearch.Shared.Models;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net.Http.Json;

namespace SqliteFulltextSearch.Shared.Client
{
    public class SearchClient
    {
        private readonly ILogger<SearchClient> _logger;
        private readonly HttpClient _httpClient;

        public SearchClient(ILogger<SearchClient> logger, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task DeleteAllDocumentsAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var response = await _httpClient
                .PostAsync("delete-all-documents", null, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(string.Format(CultureInfo.InvariantCulture,
                    "HTTP Request failed with Status: '{0}' ({1})",
                    (int)response.StatusCode,
                    response.StatusCode))
                {
                    StatusCode = response.StatusCode
                };
            }
        }

        public async Task<SearchResultsDto?> SearchAsync(string query, int from, int size, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var escapedQuery = Uri.EscapeDataString(query);

            var response = await _httpClient
                .GetAsync($"search?q={escapedQuery}&from={from}&size={size}", cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(string.Format(CultureInfo.InvariantCulture,
                    "HTTP Request failed with Status: '{0}' ({1})",
                    (int)response.StatusCode,
                    response.StatusCode))
                {
                    StatusCode = response.StatusCode
                };
            }

            return await response.Content
                .ReadFromJsonAsync<SearchResultsDto>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<SearchSuggestionsDto?> SuggestAsync(string query, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var escapedQuery = Uri.EscapeDataString(query);

            var response = await _httpClient
                .GetAsync($"suggest?q={escapedQuery}", cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(string.Format(CultureInfo.InvariantCulture,
                    "HTTP Request failed with Status: '{0}' ({1})",
                    (int)response.StatusCode,
                    response.StatusCode))
                {
                    StatusCode = response.StatusCode
                };
            }

            return await response.Content
                .ReadFromJsonAsync<SearchSuggestionsDto>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<SearchStatisticsDto>?> GetStatisticsAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();


            var response = await _httpClient
                .GetAsync($"statistics", cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(string.Format(CultureInfo.InvariantCulture,
                    "HTTP Request failed with Status: '{0}' ({1})",
                    (int)response.StatusCode,
                    response.StatusCode))
                {
                    StatusCode = response.StatusCode
                };
            }

            return await response.Content
                .ReadFromJsonAsync<List<SearchStatisticsDto>>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task UploadAsync(MultipartFormDataContent multipartFormData, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var response = await _httpClient
                .PostAsync($"upload", multipartFormData, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(string.Format(CultureInfo.InvariantCulture,
                    "HTTP Request failed with Status: '{0}' ({1})",
                    (int)response.StatusCode,
                    response.StatusCode))
                {
                    StatusCode = response.StatusCode
                };
            }
        }

        public async Task<List<SearchStatisticsDto>?> SearchStatisticsAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var response = await _httpClient
                .GetAsync("statistics", cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(string.Format(CultureInfo.InvariantCulture,
                    "HTTP Request failed with Status: '{0}' ({1})",
                    (int)response.StatusCode,
                    response.StatusCode))
                {
                    StatusCode = response.StatusCode
                };
            }

            return await response.Content
                .ReadFromJsonAsync<List<SearchStatisticsDto>>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
    }
}