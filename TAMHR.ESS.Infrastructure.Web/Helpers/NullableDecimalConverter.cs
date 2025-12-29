using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TAMHR.ESS.Infrastructure.Web.Helpers
{
    

    public class NullableDecimalConverter : JsonConverter<decimal?>
    {
        public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var value = reader.GetString();
                if (string.IsNullOrEmpty(value))
                {
                    return null;
                }
                if (decimal.TryParse(value, out var result))
                {
                    return result;
                }
            }

            return reader.TokenType == JsonTokenType.Null ? null : reader.GetDecimal();
        }

        public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
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
