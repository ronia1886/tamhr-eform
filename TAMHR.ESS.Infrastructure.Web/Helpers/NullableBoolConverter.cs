using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TAMHR.ESS.Infrastructure.Web.Helpers
{
    

    public class NullableBoolConverter : JsonConverter<bool?>
    {
        public override bool? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var value = reader.GetString();
                if (string.IsNullOrEmpty(value))
                {
                    return null;
                }
                if (bool.TryParse(value, out var result))
                {
                    return result;
                }
            }

            return reader.TokenType == JsonTokenType.Null ? null : reader.GetBoolean();
        }

        public override void Write(Utf8JsonWriter writer, bool? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteBooleanValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
