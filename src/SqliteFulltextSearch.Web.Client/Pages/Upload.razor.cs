// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Client.Infrastructure;
using ElasticsearchFulltextExample.Shared.Constants;
using Microsoft.Extensions.Localization;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Forms;

namespace ElasticsearchFulltextExample.Web.Client.Pages
{
    /// <summary>
    /// Code-Behind for the Upload.
    /// </summary>
    public partial class Upload
    {
        /// <summary>
        /// Holds the EditForm Data.
        /// </summary>
        public class UploadModel
        {
            /// <summary>
            /// Title of the Document.
            /// </summary>
            public string? Title { get; set; }

            /// <summary>
            /// Lists of Keywords, that have been added.
            /// </summary>
            public List<string> Keywords { get; set; } = new();

            /// <summary>
            /// Document Filename to be uploaded.
            /// </summary>
            public string? Filename { get; set; }

            /// <summary>
            /// File Stream, which is going to be uploaded.
            /// </summary>
            public Stream? File { get; set; }
        }

        /// <summary>
        /// The Reference to the EditForm.
        /// </summary>
        EditForm? Form { get; set; }

        /// <summary>
        /// The Reference to the File Uploader Element.
        /// </summary>
        FluentInputFile? myFileUploader = default!;

        /// <summary>
        /// The Current Upload Model representing the Form State.
        /// </summary>
        public UploadModel CurrentUpload = new UploadModel();

        /// <summary>
        /// When the Upload is done, this method is called.
        /// </summary>
        /// <param name="files">Files to be uploaded</param>
        /// <returns>An awaitable Task</returns>
        private Task OnCompletedAsync(IEnumerable<FluentInputFileEventArgs> files)
        {
            if (files == null)
            {
                return Task.CompletedTask;
            }

            var file = files.FirstOrDefault();

            if (file == null)
            {
                return Task.CompletedTask;
            }

            if (file.Stream == null)
            {
                return Task.CompletedTask;
            }

            CurrentUpload.Filename = file.Name;
            CurrentUpload.File = file.Stream;

            Form?.EditContext?.Validate();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Submits the Form and reloads the updated data.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/></returns>
        private async Task HandleValidSubmitAsync()
        {
            var validationErrors = ValidateCurrentUpload(CurrentUpload);

            if(validationErrors.Any())
            {
                return;
            }

            // The Multipart Content, we are going to upload.
            MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();

            // If there's a Title, add it as StringContent.
            if (CurrentUpload.Title != null)
            {
                multipartFormDataContent.Add(new StringContent(CurrentUpload.Title), FileUploadNames.Title);
            }

            // If there's a Filename and a File, we add it as StreamContent.
            if (CurrentUpload.Filename != null && CurrentUpload.File != null)
            {
                multipartFormDataContent.Add(new StreamContent(CurrentUpload.File), FileUploadNames.Data, CurrentUpload.Filename);
            }

            // Keywords will also be added as suggestion[0], suggestion[1], ... so the ASP.NET Core Binder turns them into a list
            if (CurrentUpload.Keywords.Any())
            {
                for (var suggestionIdx = 0; suggestionIdx < CurrentUpload.Keywords.Count; suggestionIdx++)
                {
                    multipartFormDataContent.Add(new StringContent(CurrentUpload.Keywords[suggestionIdx]), $"{FileUploadNames.Suggestions}[{suggestionIdx}]");
                }
            }

            // Keywords will be added as keyword[0], keyword[1], ... so the ASP.NET Core Binder turns them into a list
            if (CurrentUpload.Keywords.Any())
            {
                for (var keywordIdx = 0; keywordIdx < CurrentUpload.Keywords.Count; keywordIdx++)
                {
                    multipartFormDataContent.Add(new StringContent(CurrentUpload.Keywords[keywordIdx]), $"{FileUploadNames.Keywords}[{keywordIdx}]");
                }
            }

            // Upload the MultipartFormData to the Server.
            await SearchClient
                .UploadAsync(multipartFormDataContent, default)
                .ConfigureAwait(false);

            CurrentUpload = new UploadModel();
        }

        /// <summary>
        /// Submits the Form and reloads the updated data.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/></returns>
        private Task HandleDiscardAsync()
        {
            CurrentUpload = new UploadModel();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Validates a <see cref="GitRepositoryMetadataDto"/>.
        /// </summary>
        /// <param name="repository">Item to validate</param>
        /// <returns>The list of validation errors for the EditContext model fields</returns>
        private IEnumerable<ValidationError> ValidateCurrentUpload(UploadModel upload)
        {
            if (string.IsNullOrWhiteSpace(upload.Title))
            {
                yield return new ValidationError
                {
                    PropertyName = nameof(upload.Title),
                    ErrorMessage = Loc.GetString("Validation_IsRequired", nameof(upload.Title))
                };
            }

            if (upload.Keywords.Count == 0)
            {
                yield return new ValidationError
                {
                    PropertyName = nameof(upload.Keywords),
                    ErrorMessage = Loc.GetString("Validation_IsRequired", nameof(upload.Keywords))
                };
            }

            if (string.IsNullOrWhiteSpace(upload.Filename))
            {
                yield return new ValidationError
                {
                    PropertyName = nameof(upload.Filename),
                    ErrorMessage = Loc.GetString("Validation_IsRequired", nameof(upload.Filename))
                };
            }
        }
    }
}