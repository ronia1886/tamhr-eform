using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TAMHR.ESS.Infrastructure.Web.Helpers
{
    public class NullableIntConverter : JsonConverter<int?>
    {
        public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var value = reader.GetString();
                if (string.IsNullOrEmpty(value))
                {
                    return null;
                }
                if (int.TryParse(value, out int result))
                {
                    return result;
                }
            }

            return reader.TokenType == JsonTokenType.Null ? null : reader.GetInt32();
        }

        public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteNumberValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
