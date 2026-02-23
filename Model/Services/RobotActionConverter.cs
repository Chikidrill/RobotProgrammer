using Model.RobotActions;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Model.Services;

public class RobotActionConverter : JsonConverter<RobotAction>
{
    public override RobotAction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("DisplayType", out var displayType))
            throw new JsonException("Missing DisplayType");

        RobotAction action = displayType.GetString() switch
        {
            "Move" => JsonSerializer.Deserialize<MoveAction>(root.GetRawText(), options)!,
            "Wait" => JsonSerializer.Deserialize<WaitAction>(root.GetRawText(), options)!,
            _ => JsonSerializer.Deserialize<CustomAction>(root.GetRawText(), options)!
        };

        return action;
    }

    public override void Write(Utf8JsonWriter writer, RobotAction value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case MoveAction move:
                JsonSerializer.Serialize(writer, move, options);
                break;
            case WaitAction wait:
                JsonSerializer.Serialize(writer, wait, options);
                break;
            case CustomAction custom:
                JsonSerializer.Serialize(writer, custom, options);
                break;
            default:
                throw new NotSupportedException("Unknown RobotAction type");
        }
    }
}
