using System;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace TAMHR.ESS.Infrastructure.Web.Helpers
{
    public class NullableGuidConverter : JsonConverter<Guid?>
    {
        public override Guid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var value = reader.GetString();
                if (string.IsNullOrEmpty(value))
                {
                    return null;
                }
                if (Guid.TryParse(value, out var result))
                {
                    return result;
                }
            }

            return reader.TokenType == JsonTokenType.Null ? null : reader.GetGuid();
        }

        public override void Write(Utf8JsonWriter writer, Guid? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStringValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
