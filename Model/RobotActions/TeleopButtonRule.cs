using System.Text.Json.Serialization;

namespace Model.RobotActions;

public class TeleopButtonRule : ContainerAction
{
    public string Button { get; set; } = "TRIANGLE";

    public TriggerMode TriggerMode { get; set; } = TriggerMode.WhilePressed;

    public override string ActionType => "TeleopButtonRule";

    [JsonIgnore]
    public override string DisplayType => $"{Button} / {GetModeName()}";

    public override string GenerateCode()
    {
        string condition = TriggerMode switch
        {
            TriggerMode.WhilePressed => $"ps4.Button({Button})",
            TriggerMode.WhileNotPressed => $"!ps4.Button({Button})",
            _ => $"ps4.Button({Button})"
        };

        string code = $"if ({condition}) {{\n";

        foreach (var child in Children)
            code += child.GenerateCode() + "\n";

        code += "}\n";

        return code;
    }

    private string GetModeName()
    {
        return TriggerMode switch
        {
            TriggerMode.WhilePressed => "пока нажата",
            TriggerMode.WhileNotPressed => "пока не нажата",
            _ => TriggerMode.ToString()
        };
    }
}