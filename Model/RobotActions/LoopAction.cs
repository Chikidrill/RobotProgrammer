using Model.Services;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Model.RobotActions;

public class LoopAction : ContainerAction
{
    public int RepeatCount { get; set; } = 2;

    public override string ActionType => "LoopAction";

    [JsonIgnore]
    public override string DisplayType => $"Цикл x{RepeatCount}";

    public override string GenerateCode()
    {
        var code = $"{{\nfor(int i=0;i<{RepeatCount};i++){{\n";

        foreach (var child in Children)
            code += child.GenerateCode();

        code += "}\n";
        return code;
    }

    public override ObservableCollection<ActionParameter> GetParameters()
    {
        return new()
        {
            new ActionParameter
            {
                Name = "Повторов",
                Value = RepeatCount
            }
        };
    }

    public override void ApplyParameters(IEnumerable<ActionParameter> parameters)
    {
        foreach (var p in parameters)
            if (p.Name == "Повторов")
                RepeatCount = p.Value;
    }
}