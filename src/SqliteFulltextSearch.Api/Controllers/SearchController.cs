// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Api.Configuration;
using ElasticsearchFulltextExample.Api.Infrastructure.Errors;
using ElasticsearchFulltextExample.Api.Infrastructure.Exceptions;
using ElasticsearchFulltextExample.Api.Models;
using ElasticsearchFulltextExample.Api.Services;
using ElasticsearchFulltextExample.Shared.Infrastructure;
using ElasticsearchFulltextExample.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ElasticsearchFulltextExample.Api.Controllers
{
    public class SearchController : ControllerBase
    {
        private readonly ILogger<SearchController> _logger;

        private readonly ApplicationOptions _applicationOptions;
        private readonly DocumentService _documentService;
        private readonly SqliteSearchService _elasticsearchService;
        private readonly ExceptionToErrorMapper _exceptionToApplicationErrorMapper;

        public SearchController(ILogger<SearchController> logger, IOptions<ApplicationOptions> applicationOptions, DocumentService documentService, SqliteSearchService elasticsearchService, ExceptionToErrorMapper exceptionToApplicationErrorMapper)
        {
            _logger = logger;
            _applicationOptions = applicationOptions.Value;
            _documentService = documentService;
            _elasticsearchService = elasticsearchService;
            _exceptionToApplicationErrorMapper = exceptionToApplicationErrorMapper;
        }

        [HttpGet("/raw/{documentId}")]
        public async Task<IActionResult> GetFileById([FromRoute(Name = "documentId")] int documentId, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            try
            {
                if (!ModelState.IsValid)
                {
                    throw new InvalidModelStateException
                    {
                        ModelStateDictionary = ModelState
                    };
                }

                if (_logger.IsDebugEnabled())
                {
                    _logger.LogDebug($"Downloading File with Document ID '{documentId}'");
                }

                var fileInformation = await _documentService
                    .GetFileInformationByDocumentIdAsync(documentId, cancellationToken)
                    .ConfigureAwait(false);

                return File(fileContents: fileInformation.Data, contentType: fileInformation.ContentType, fileDownloadName: fileInformation.Filename);
            }
            catch (Exception exception)
            {
                return _exceptionToApplicationErrorMapper.CreateApplicationErrorResult(HttpContext, exception);
            }
        }

        [HttpPost("/upload")]
        public async Task<IActionResult> Upload(
            [FromForm(Name = "title")] string title,
            [FromForm(Name = "suggestions")] List<string>? suggestions,
            [FromForm(Name = "keywords")] List<string>? keywords,
            [FromForm(Name = "data")] IFormFile data,
            CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            try
            {
                if (!ModelState.IsValid)
                {
                    throw new InvalidModelStateException
                    {
                        ModelStateDictionary = ModelState
                    };
                }

                var fileBytes = await GetBytesAsync(data).ConfigureAwait(false);

                await _documentService
                    .CreateDocumentAsync(title, data.FileName, fileBytes, suggestions, keywords, Constants.Users.DataConversionUserId, cancellationToken)
                    .ConfigureAwait(false);

                return Created();
            }
            catch (Exception exception)
            {
                return _exceptionToApplicationErrorMapper.CreateApplicationErrorResult(HttpContext, exception);
            }
        }

        private async Task<byte[]> GetBytesAsync(IFormFile formFile)
        {
            using (var memoryStream = new MemoryStream())
            {
                await formFile.CopyToAsync(memoryStream);

                return memoryStream.ToArray();
            }
        }

        [HttpGet]
        [Route("/statistics")]
        public async Task<IActionResult> GetSearchStatistics(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            try
            {
                var searchStatistics = await _elasticsearchService.GetSearchStatisticsAsync(cancellationToken);

                var searchStatisticsDto = Convert(searchStatistics);

                return Ok(searchStatisticsDto);
            }
            catch (Exception e)
            {
                if (_logger.IsErrorEnabled())
                {
                    _logger.LogError(e, "An unhandeled exception occured");
                }

                return StatusCode(500, "An internal Server Error occured");
            }
        }

        private List<SearchStatisticsDto> Convert(List<SearchStatistics> source)
        {
            return source
                .Select(x => Convert(x))
                .ToList();
        }

        private SearchStatisticsDto Convert(SearchStatistics source)
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

        [HttpGet]
        [Route("/search")]
        public async Task<IActionResult> Query([FromQuery(Name = "q")] string query, [FromQuery(Name = "from")] int from, [FromQuery(Name = "size")] int size, CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new InvalidModelStateException
                    {
                        ModelStateDictionary = ModelState
                    };
                }

                if (_logger.IsDebugEnabled())
                {
                    _logger.LogDebug("Executing SearchQuery '{SearchQuery}'", query);
                }

                var searchResults = await _elasticsearchService
                    .SearchAsync(query, from, size, cancellationToken)
                    .ConfigureAwait(false);

                var searchResultsDto = Convert(searchResults);

                return Ok(searchResultsDto);
            }
            catch (Exception exception)
            {
                return _exceptionToApplicationErrorMapper.CreateApplicationErrorResult(HttpContext, exception);
            }
        }

        private SearchResultsDto Convert(SearchResults source)
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

        private List<SearchResultDto> Convert(List<SearchResult> source)
        {
            return source
                .Select(searchResult => Convert(searchResult))
                .ToList();
        }

        private SearchResultDto Convert(SearchResult source)
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

        [HttpGet]
        [Route("/suggest")]
        public async Task<IActionResult> Suggest([FromQuery(Name = "q")] string query, CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new InvalidModelStateException
                    {
                        ModelStateDictionary = ModelState
                    };
                }

                if (_logger.IsDebugEnabled())
                {
                    _logger.LogDebug("Getting Suggestions '{SearchSuggestions}'", query);
                }

                var searchSuggestions = await _elasticsearchService
                    .SuggestAsync(query, cancellationToken)
                    .ConfigureAwait(false);

                var searchSuggestionsDto = Convert(searchSuggestions);
                
                return Ok(searchSuggestions);
            }
            catch (Exception exception)
            {
                return _exceptionToApplicationErrorMapper.CreateApplicationErrorResult(HttpContext, exception);
            }
        }

        [HttpPost]
        [Route("/delete-index")]
        public async Task<IActionResult> DeleteIndex(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            try
            {
                await _elasticsearchService.DeleteIndexAsync(cancellationToken);

                return Ok();
            }
            catch (Exception e)
            {
                if (_logger.IsErrorEnabled())
                {
                    _logger.LogError(e, "Failed to delete index");
                }

                return StatusCode(500);
            }
        }

        [HttpPost]
        [Route("/delete-pipeline/{pipeline}")]
        public async Task<IActionResult> DeletePipeline([FromRoute(Name = "pipeline")] string pipeline, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            try
            {
                await _elasticsearchService.DeletePipelineAsync(pipeline, cancellationToken);

                return Ok();
            }
            catch (Exception e)
            {
                if (_logger.IsErrorEnabled())
                {
                    _logger.LogError(e, "Failed to delete pipeline");
                }

                return StatusCode(500);
            }
        }

        [HttpPost]
        [Route("/delete-all-documents")]
        public async Task<IActionResult> DeleteAllDocuments(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            try
            {
                await _elasticsearchService.DeleteAllAsync(cancellationToken);

                return Ok();
            }
            catch (Exception e)
            {
                if (_logger.IsErrorEnabled())
                {
                    _logger.LogError(e, "An unhandeled exception occured");
                }

                return StatusCode(500, "An internal Server Error occured");
            }
        }

        [HttpPost]
        [Route("/create-index")]
        public async Task<IActionResult> CreateIndex(CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new InvalidModelStateException
                    {
                        ModelStateDictionary = ModelState
                    };
                }

                if (_logger.IsDebugEnabled())
                {
                    _logger.LogDebug("Creating Search Index");
                }

                await _elasticsearchService
                    .CreateIndexAsync(cancellationToken)
                    .ConfigureAwait(false);

                return Ok();
            }
            catch (Exception exception)
            {
                return _exceptionToApplicationErrorMapper.CreateApplicationErrorResult(HttpContext, exception);
            }
        }

        [HttpPost]
        [Route("/create-pipeline")]
        public async Task<IActionResult> CreatePipeline(CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new InvalidModelStateException
                    {
                        ModelStateDictionary = ModelState
                    };
                }

                if (_logger.IsDebugEnabled())
                {
                    _logger.LogDebug("Creating Search Pipeline");
                }

                await _elasticsearchService
                    .CreatePipelineAsync(cancellationToken)
                    .ConfigureAwait(false);

                return Ok();
            }
            catch (Exception exception)
            {
                return _exceptionToApplicationErrorMapper.CreateApplicationErrorResult(HttpContext, exception);
            }
        }

        private SearchSuggestionsDto Convert(SearchSuggestions source)
        {
            return new SearchSuggestionsDto
            {
                Query = source.Query,
                Results = Convert(source.Results)
            };
        }

        private List<SearchSuggestionDto> Convert(List<SearchSuggestion> source)
        {
            return source
                .Select(suggestion => Convert(suggestion))
                .ToList();
        }

        private SearchSuggestionDto Convert(SearchSuggestion source)
        {
            return new SearchSuggestionDto
            {
                Text = source.Text,
                Highlight = source.Highlight
            };
        }
    }
}