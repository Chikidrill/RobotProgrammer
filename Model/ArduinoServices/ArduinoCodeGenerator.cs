using Model.RobotActions;
using System.Text;

namespace Model.ArduinoServices;

public class ArduinoCodeGenerator
{
    public string GenerateCode(RobotProgram program)
    {
        var includesBuilder = new StringBuilder();
        var globalsBuilder = new StringBuilder();
        var functionsBuilder = new StringBuilder();
        var setupBuilder = new StringBuilder();
        var autonomousBuilder = new StringBuilder();
        var teleopBuilder = new StringBuilder();

        foreach (var includeCode in program.Includes
                     .Where(include => include.IsEnabled)
                     .Select(include => include.GenerateCode())
                     .Where(code => !string.IsNullOrWhiteSpace(code))
                     .Distinct())
        {
            includesBuilder.AppendLine(includeCode);
        }

        if (!string.IsNullOrWhiteSpace(program.GlobalCode))
        {
            globalsBuilder.AppendLine(program.GlobalCode.Trim());
            globalsBuilder.AppendLine();
        }

        foreach (var variable in program.Variables)
            globalsBuilder.AppendLine(variable.GetDeclarationCode());

        foreach (var function in program.Functions)
            functionsBuilder.AppendLine(function.GenerateCode());

        if (!string.IsNullOrWhiteSpace(program.SetupBaseCode))
            setupBuilder.AppendLine(Indent(program.SetupBaseCode.Trim(), 2));

        setupBuilder.AppendLine();
        setupBuilder.AppendLine("  // ===== User Setup =====");

        foreach (var action in program.Setup)
            setupBuilder.AppendLine(Indent(action.GenerateCode(), 2));

        foreach (var action in program.Autonomous)
            autonomousBuilder.AppendLine(Indent(action.GenerateCode(), 2));

        teleopBuilder.AppendLine("  // Always running");
        foreach (var action in program.Teleop.AlwaysRunning)
            teleopBuilder.AppendLine(Indent(action.GenerateCode(), 2));

        teleopBuilder.AppendLine();
        teleopBuilder.AppendLine("  // Button rules");
        foreach (var rule in program.Teleop.ButtonRules)
            teleopBuilder.AppendLine(Indent(rule.GenerateCode(), 2));

        return $@"
{includesBuilder}
{globalsBuilder}
// ===== User Functions =====
{functionsBuilder}

void setup() {{
{setupBuilder}
}}

void loop() {{
{Indent(program.LoopBaseCode, 2)}

  if (autoMode) {{
    RunAutonomous();
  }} else {{
    RunTeleop();
  }}

{Indent(program.LoopEndCode, 2)}
}}

void RunAutonomous() {{
{autonomousBuilder}
}}

void RunTeleop() {{
{teleopBuilder}
}}
".TrimStart();
    }

    public string SaveToFile(RobotProgram program)
    {
        string code = GenerateCode(program);

        string folderPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "robot");

        Directory.CreateDirectory(folderPath);

        string filePath = Path.Combine(folderPath, "robot.ino");

        File.WriteAllText(filePath, code);

        return folderPath;
    }

    private static string Indent(string? code, int spaces)
    {
        if (string.IsNullOrWhiteSpace(code))
            return string.Empty;

        string prefix = new(' ', spaces);

        var lines = code
            .Replace("\r\n", "\n")
            .Replace("\r", "\n")
            .Split('\n');

        return string.Join(
            Environment.NewLine,
            lines.Select(line =>
                string.IsNullOrWhiteSpace(line)
                    ? string.Empty
                    : prefix + line));
    }
}