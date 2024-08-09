namespace ElasticsearchFulltextExample.Web.Client.Components
{
    public class AutocompleteSearchEventArgs
    {
        /// <summary>
        /// Gets or sets the text to search.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of items to display.
        /// </summary>
        public List<string>? Items { get; set; }
    }
}
