using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace RobotProgrammer.Model;

public class WaitAction : RobotAction
{
    public WaitAction()
    {
        Parameters = new ObservableCollection<ActionParameter>
        {
            new() { Name = "DurationMs", Value = 500 }
        };
    }
    public ObservableCollection<ActionParameter> Parameters { get; set; }
    public override ObservableCollection<ActionParameter> GetParameters() => Parameters;
    public override string ActionType => "WaitAction";
    public override string GenerateCode()
         => $"delay({Parameters.First(p => p.Name == "DurationMs").Value});";
    [JsonIgnore]
    public override string DisplayType => GetType().Name.Replace("Action", "");

    public override void ApplyParameters(IEnumerable<ActionParameter> parameters)
    {
        foreach (var p in parameters)
        {
            var target = Parameters.FirstOrDefault(x => x.Name == p.Name);
            if (target != null) target.Value = p.Value;
        }
    }
}
