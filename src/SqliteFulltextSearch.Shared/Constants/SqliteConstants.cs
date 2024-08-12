namespace SqliteFulltextSearch.Shared.Constants
{
    public static class SqliteConstants
    {
        public static class Highlighter
        {
            public const string HighlightStartTag = "match→";
            
            public const string HighlightEndTag = "←match";
        }

        public static class Formats
        {
            public static string DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

            public static string[] DateTimeFormats = ["yyyy-MM-dd HH:mm:ss.fff", "yyyy-MM-dd"];
        }
    }
}
