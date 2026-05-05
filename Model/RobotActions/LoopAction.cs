using Model.Services;
using System.Collections.ObjectModel;
using System.Text;
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
        var builder = new StringBuilder();

        builder.AppendLine($"  for (int i = 0; i < {RepeatCount}; i++) {{");

        foreach (var child in Children)
        {
            var childCode = child.GenerateCode();

            foreach (var line in childCode.Split(
                         new[] { "\r\n", "\n" },
                         StringSplitOptions.None))
            {
                if (!string.IsNullOrWhiteSpace(line))
                    builder.AppendLine("  " + line);
            }
        }

        builder.AppendLine("  }");

        return builder.ToString();
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