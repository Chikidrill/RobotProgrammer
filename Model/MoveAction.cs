using System.Collections.ObjectModel;

namespace RobotProgrammer.Model;

public class MoveAction : RobotAction
{
    public MoveAction()
    {
        Parameters = new ObservableCollection<ActionParameter>
        {
            new() { Name = "SpeedLeft", Value = 100 },
            new() { Name = "SpeedRight", Value = 100 },
            new() { Name = "DurationMs", Value = 1000 }
        };
    }
    public override string ActionType => "MoveAction";
    public override string GenerateCode()
        => $@"
    prizm.setMotorSpeeds({Parameters.First(p => p.Name == "SpeedLeft").Value}, 
                          {Parameters.First(p => p.Name == "SpeedRight").Value});
    delay({Parameters.First(p => p.Name == "DurationMs").Value});
    prizm.setMotorSpeeds(0,0);";

    public ObservableCollection<ActionParameter> Parameters { get; set; }
    public override ObservableCollection<ActionParameter> GetParameters() => Parameters;
    public override void ApplyParameters(IEnumerable<ActionParameter> parameters)
    {
        foreach (var p in parameters)
        {
            var target = Parameters.FirstOrDefault(x => x.Name == p.Name);
            if (target != null) target.Value = p.Value;
        }
    }
}
