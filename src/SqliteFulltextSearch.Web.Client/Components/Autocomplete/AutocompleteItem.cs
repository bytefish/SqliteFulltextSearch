namespace ElasticsearchFulltextExample.Web.Client.Components
{
    public class AutocompleteItem
    {
        public required string Html { get; set; } = string.Empty;

        public required string Text { get; set; } = string.Empty;

        public bool IsSelected { get; set; }
    }
}
