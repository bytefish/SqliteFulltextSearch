// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;
using System.Text.Json;
using SqliteFulltextSearch.Shared.Constants;
using System.Globalization;

namespace SqliteFulltextSearch.Shared.Infrastructure
{
    public class SqliteDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();

            if (value == null)
            {
                return default;
            }

            return DateTime.ParseExact(value, SqliteConstants.Formats.DateTimeFormats, CultureInfo.InvariantCulture);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(SqliteConstants.Formats.DateTimeFormat));
        }
    }

}
