using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SAAE.Editor.Converters;

public class CultureJsonConverter : JsonConverter<CultureInfo> {
    public override CultureInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.String) {
            throw new JsonException();
        }

        var value = reader.GetString();
        if (value == null) {
            return null;
        }

        return new CultureInfo(value);
    }

    public override void Write(Utf8JsonWriter writer, CultureInfo value, JsonSerializerOptions options) {
        writer.WriteStringValue(value.Name);
    }
}