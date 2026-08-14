using System;
using System.Collections.Generic;
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
        
        // read blocks
        foreach (JsonElement blockElement in blocksProperty.EnumerateArray()) {
            string name = blockElement.GetProperty("name").GetString()!;
            bool isBarrier = blockElement.GetProperty("isBarrier").GetBoolean();
            string source = blockElement.GetProperty("source").GetString()!;
            List<IoItem> inputs = [];
            foreach (JsonElement input in blockElement.GetProperty("inputs").EnumerateArray()) {
                string inputName = input.GetProperty("name").GetString()!;
                bool isSigned = input.GetProperty("isSigned").GetBoolean();
                int width = input.GetProperty("width").GetInt32();
                inputs.Add(new IoItem(inputName,width, isSigned));
            }
            List<IoItem> outputs = [];
            foreach (JsonElement output in blockElement.GetProperty("outputs").EnumerateArray()) {
                string outputName = output.GetProperty("name").GetString()!;
                bool isSigned = output.GetProperty("isSigned").GetBoolean();
                int width = output.GetProperty("width").GetInt32();
                outputs.Add(new IoItem(outputName,width, isSigned));
            }
            DesignBlock block = new(name, inputs, outputs, isBarrier, source);
            design.Blocks.Add(block);
        }
        
        // read connections
        foreach (JsonElement connectionElement in connectionsProperty.EnumerateArray()) {
            string startName = connectionElement.GetProperty("start").GetString()!;
            string endName = connectionElement.GetProperty("end").GetString()!;
            int startOutputIndex = connectionElement.GetProperty("startOutputIndex").GetInt32();
            int endInputIndex = connectionElement.GetProperty("endInputIndex").GetInt32();
            DesignBlock? start = design.Blocks.FirstOrDefault(x => x.Name == startName);
            DesignBlock? end = design.Blocks.FirstOrDefault(x => x.Name == endName);
            if (start == null || end == null) {
                throw new JsonException("Could not find start or end block from connection");
            }

            design.Connections.Add(new Connection(start, startOutputIndex, end, endInputIndex));
        }
        
        return design;
    }
}