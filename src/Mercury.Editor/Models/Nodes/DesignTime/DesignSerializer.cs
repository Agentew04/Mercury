using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Mercury.Editor.Models.Nodes.DesignTime;

/// <summary>
/// Class to serialize and deserialize designs into JSON. Mainly equals to actual classes format, but with some
/// caveats.
/// </summary>
public static class DesignSerializer {

    public static void Serialize(Design design, Stream s, JsonSerializerOptions? options = null) {

        JsonArray blocks = new(design.Blocks.Select(x => new JsonObject() {
            ["name"] = x.Name,
            ["isBarrier"] = x.IsBarrier,
            ["source"] = x.Source,
            ["inputs"] = new JsonArray(x.Inputs.Select(y => new JsonObject() {
                ["name"] = y.Name,
                ["isSigned"] = y.Signed,
                ["width"] = y.Size
            }).ToArray()),
            ["outputs"] = new JsonArray(x.Outputs.Select(y => new JsonObject() {
                ["name"] = y.Name,
                ["isSigned"] = y.Signed,
                ["width"] = y.Size
            }).ToArray())
        }).ToArray());
        JsonArray connections = new(design.Connections.Select(x => new JsonObject() {
            ["start"] = x.Start.Name,
            ["end"] = x.End.Name,
            ["startOutputIndex"] = x.StartOutputIndex,
            ["endInputIndex"] = x.EndInputIndex
        }).ToArray());

        JsonObject root = new() {
            ["blocks"] = blocks,
            ["connections"] = connections
        };

        using Utf8JsonWriter writer = new(s);
        root.WriteTo(writer, options);
    }

    public static string Serialize(Design design, JsonSerializerOptions? options = null) {
        using MemoryStream ms = new();
        Serialize(design, ms, options);
        ms.Seek(0,SeekOrigin.Begin);
        using StreamReader sr = new(ms, Encoding.UTF8);
        return sr.ReadToEnd();
    }

    public static Design Deserialize(string json) {
        using JsonDocument document = JsonDocument.Parse(json);
        return ParseDocument(document);
    }

    public static Design Deserialize(Stream stream) {
        using JsonDocument document = JsonDocument.Parse(stream);
        return ParseDocument(document);
    }

    private static Design ParseDocument(JsonDocument document) {
        JsonElement blocksProperty = document.RootElement.GetProperty("blocks");
        JsonElement connectionsProperty = document.RootElement.GetProperty("connections");

        Design design = new();
        foreach (JsonElement blockElement in blocksProperty.EnumerateArray()) {
            DesignBlock block = new();
            string name = blockElement.GetProperty("name").GetString()!;
        }
            
    }
}