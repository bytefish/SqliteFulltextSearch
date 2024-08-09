// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Shared.Constants;
using ElasticsearchFulltextExample.Shared.Infrastructure;
using ElasticsearchFulltextExample.Shared.Models;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net.Http.Json;

namespace ElasticsearchFulltextExample.Shared.Client
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

        public async Task DeleteSearchIndexAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var response = await _httpClient
                .PostAsync("delete-index", null, cancellationToken)
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
        public async Task DeleteSearchPipelineAsync(CancellationToken cancellationToken)
        {
            await DeletePipelineAsync(ElasticConstants.Pipelines.Attachments, cancellationToken).ConfigureAwait(false);
        }


        public async Task DeletePipelineAsync(string pipeline, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var response = await _httpClient
                .PostAsync($"delete-pipeline/{pipeline}", null, cancellationToken)
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

        public async Task CreateSearchIndexAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var response = await _httpClient
                .PostAsync("create-index", null, cancellationToken)
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

        public async Task CreateSearchPipelineAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var response = await _httpClient
                .PostAsync("create-pipeline", null, cancellationToken)
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