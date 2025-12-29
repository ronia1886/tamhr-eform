using System;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace TAMHR.ESS.Infrastructure.Web.Helpers
{
    
    public class NullableDateTimeConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var dateString = reader.GetString();
                if (string.IsNullOrEmpty(dateString))
                {
                    return null;
                }
                if (DateTime.TryParse(dateString, out var date))
                {
                    return date;
                }
            }
            return reader.TokenType == JsonTokenType.Null ? null : (DateTime?)reader.GetDateTime();
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStringValue(value.Value.ToString("o")); // Menggunakan format ISO 8601
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
