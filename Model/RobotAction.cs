using System.Text.Json.Serialization;

namespace RobotProgrammer.Model;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(MoveAction), "Move")]
[JsonDerivedType(typeof(WaitAction), "Wait")]
public abstract class RobotAction
{
    // Общие параметры (оставляем)
    public int SpeedLeft { get; set; }
    public int SpeedRight { get; set; }
    public int DurationMs { get; set; }

    [JsonIgnore]
    public string DisplayType => GetType().Name.Replace("Action", "");
    public abstract string GenerateCode();
}
