using System.Text.Json.Serialization;

namespace RobotProgrammer.Model;

public class WaitAction : RobotAction
{
    public WaitAction() { }
    public override string ActionType => "WaitAction";
    public override string GenerateCode()
    {
        return $"delay({DurationMs});";
    }
    [JsonIgnore]
    public string DisplayType => GetType().Name.Replace("Action", "");
}
