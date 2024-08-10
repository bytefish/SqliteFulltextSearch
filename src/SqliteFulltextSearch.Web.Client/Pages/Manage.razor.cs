// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using SqliteFulltextSearch.Shared.Client;

namespace SqliteFulltextSearch.Web.Client.Pages
{
    public partial class Manage
    {
        /// <summary>
        /// Deletes all Documents from Index.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/></returns>
        private async Task HandleDeleteAllDocumentsAsync()
        {
            await SearchClient.DeleteAllDocumentsAsync(default);
        }
    }
}