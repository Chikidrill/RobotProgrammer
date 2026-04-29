using Model.RobotActions;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Model.Services;

public class RobotActionConverter : JsonConverter<RobotAction>
{
    public override RobotAction Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        string? actionType = null;

        // Новый правильный формат
        if (root.TryGetProperty("ActionType", out var actionTypeProperty))
        {
            actionType = actionTypeProperty.GetString();
        }
        // Поддержка старых сохранений, где был DisplayType
        else if (root.TryGetProperty("DisplayType", out var displayTypeProperty))
        {
            var displayType = displayTypeProperty.GetString();

            actionType = displayType switch
            {
                "Move" => "MoveAction",
                "Wait" => "WaitAction",
                _ => "CustomAction"
            };
        }

        if (string.IsNullOrWhiteSpace(actionType))
            throw new JsonException("Missing ActionType");

        return actionType switch
        {
            "MoveAction" =>
                JsonSerializer.Deserialize<MoveAction>(root.GetRawText(), options)
                ?? throw new JsonException("Cannot deserialize MoveAction"),

            "WaitAction" =>
                JsonSerializer.Deserialize<WaitAction>(root.GetRawText(), options)
                ?? throw new JsonException("Cannot deserialize WaitAction"),

            "CustomAction" =>
                JsonSerializer.Deserialize<CustomAction>(root.GetRawText(), options)
                ?? throw new JsonException("Cannot deserialize CustomAction"),

            "LoopAction" =>
                JsonSerializer.Deserialize<LoopAction>(root.GetRawText(), options)
                ?? throw new JsonException("Cannot deserialize LoopAction"),

            "ConditionalAction" =>
                JsonSerializer.Deserialize<ConditionalAction>(root.GetRawText(), options)
                ?? throw new JsonException("Cannot deserialize ConditionalAction"),
            "BranchAction" =>
                JsonSerializer.Deserialize<BranchAction>(root.GetRawText(), options)
                ?? throw new JsonException("Cannot deserialize BranchAction"),

            _ => throw new JsonException($"Unknown RobotAction type: {actionType}")
        };
    }

    public override void Write(
        Utf8JsonWriter writer,
        RobotAction value,
        JsonSerializerOptions options)
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

            case LoopAction loop:
                JsonSerializer.Serialize(writer, loop, options);
                break;

            case ConditionalAction conditional:
                JsonSerializer.Serialize(writer, conditional, options);
                break;
            case BranchAction branch:
                JsonSerializer.Serialize(writer, branch, options);
                break;
            default:
                throw new NotSupportedException(
                    $"Unknown RobotAction type: {value.GetType().Name}");
        }
    }
}