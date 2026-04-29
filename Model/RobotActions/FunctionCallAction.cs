using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Model.RobotActions;

public class FunctionCallAction : RobotAction
{
    public string FunctionName { get; set; } = "";

    public ObservableCollection<ParameterItem> Arguments { get; set; } = new();

    public override string ActionType => "FunctionCallAction";

    [JsonIgnore]
    public override string DisplayType =>
        string.IsNullOrWhiteSpace(FunctionName)
            ? "Вызов функции"
            : $"Вызов: {FunctionName}";

    public override string GenerateCode()
    {
        var args = string.Join(", ", Arguments.Select(a => a.Value));
        return $"{FunctionName}({args});";
    }
}