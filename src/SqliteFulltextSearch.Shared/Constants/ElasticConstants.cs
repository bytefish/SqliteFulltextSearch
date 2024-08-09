// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchFulltextExample.Shared.Constants
{
    public static class ElasticConstants
    {
        /// <summary>
        /// Pipeline Constants, such as Names.
        /// </summary>
        public static class Pipelines
        {
            /// <summary>
            /// Name of the Attachments Processor Pipeline.
            /// </summary>
            public const string Attachments = "attachments";
        }
        
        /// <summary>
        /// Pipeline Processor Constants, such as Names.
        /// </summary>
        public static class PipelineProcessors
        {
            /// <summary>
            /// Name of the Attachments Processor Pipeline.
            /// </summary>
            public const string HtmlStrip = "html_strip";
        }

        /// <summary>
        /// Constants for Highlighters.
        /// </summary>
        public static class Highlighter
        {
            /// <summary>
            /// A tag used to find the highlightning start position.
            /// </summary>
            public static readonly string HighlightStartTag = "elasticsearch→";

            /// <summary>
            /// A tag used to find the highlightning end position.
            /// </summary>
            public static readonly string HighlightEndTag = "←elasticsearch";
        }

        /// <summary>
        /// Property Names for the Elasticsearch Documents.
        /// </summary>
        public static class DocumentNames
        {
            /// <summary>
            /// Document ID.
            /// </summary>
            public const string Id = "id";

            /// <summary>
            /// Title.
            /// </summary>
            public const string Title = "title";

            /// <summary>
            /// Filename.
            /// </summary>
            public const string Filename = "filename";

            /// <summary>
            /// Binary Data.
            /// </summary>
            public const string Data = "data";

            /// <summary>
            /// List of Keywords.
            /// </summary>
            public const string Keywords = "keywords";

            /// <summary>
            /// List of Suggestions.
            /// </summary>
            public const string Suggestions = "suggestions";

            /// <summary>
            /// Date the File has been indexed at.
            /// </summary>
            public const string IndexedOn = "indexed_on";

            /// <summary>
            /// Attachment.
            /// </summary>
            public const string Attachment = "attachment";
        }

        /// <summary>
        /// Attachment Metadata populated by the Elasticsearch Plugin, which 
        /// in turn uses Apache Tika for extracting data off of files. The 
        /// Properties have been taken off of Apache Tika.
        /// </summary>
        public static class AttachmentNames
        {
            public const string Content = "content";

            public const string Title = "title";

            public const string Author = "author";

            public const string Date = "date";

            public const string Keywords = "keywords";

            public const string ContentType = "content_type";

            public const string ContentLength = "content_length";

            public const string Language = "language";

            public const string Modified = "modified";

            public const string Format = "format";

            public const string Identifier = "identifier";

            public const string Contributor = "contributor";

            public const string Coverage = "converage";

            public const string Modifier = "modifier";

            public const string CreatorTool = "creator_tool";

            public const string Publisher = "publisher";

            public const string Relation = "relation";

            public const string Rights = "rights";

            public const string Source = "source";

            public const string Type = "type";

            public const string Description = "description";

            public const string PrintDate = "print_date";

            public const string MetadataDate = "metadata_date";

            public const string Latitude = "latitude";

            public const string Longitude = "longitude";

            public const string Altitude = "altitude";

            public const string Rating = "rating";

            public const string Comments = "comments";
        }
    }
}
