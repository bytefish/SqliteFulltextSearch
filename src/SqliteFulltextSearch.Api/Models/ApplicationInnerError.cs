using System.Text.Json.Serialization;

namespace ElasticsearchFulltextExample.Api.Models
{
    /// <summary>
    /// Class representing implementation specific debugging information to help determine the cause of the error.
    /// </summary>
    public class ApplicationInnerError
    {
        /// <summary>
        /// The value for the message name/value pair is a non-empty, language-dependent, human-readable string describing 
        /// the error. The Content-Language header MUST contain the language code from[RFC5646] corresponding to the 
        /// language in which the value for message is written.It cannot be null.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// The value for the target name/value pair is a potentially empty string indicating the target of the 
        /// error (for example, the name of the property in error). It can be null.
        /// </summary>
        public string? Target { get; set; }

        /// <summary>
        /// Additional Debugging information, such as a Stack Trace.
        /// </summary>
        public string? StackTrace { get; set; }

        /// <summary>
        /// A nested inner error.
        /// </summary>
        public ApplicationInnerError? InnerError { get; set; }

        /// <summary>
        /// Additional Properties.
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object> AdditionalProperties { get; set; } = new();
    }
}