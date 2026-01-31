using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace RobotProgrammer.Model;

public abstract class RobotAction
{
    // Общие параметры (оставляем)
    public int SpeedLeft { get; set; }
    public int SpeedRight { get; set; }
    public int DurationMs { get; set; }

    public virtual string DisplayType => GetType().Name.Replace("Action", "");
    public abstract string GenerateCode();
}
