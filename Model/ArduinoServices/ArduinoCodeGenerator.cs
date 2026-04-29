using Model.RobotActions;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Model.ArduinoServices;

public class ArduinoCodeGenerator
{
    public string GenerateCode(
        IEnumerable<RobotAction> setup,
        IEnumerable<RobotAction> autonomous,
        TeleopProgram teleop,
        IEnumerable<ProgramVariable> variables,
        IEnumerable<ProgramFunction> functions)
    {
        var globalsBuilder = new StringBuilder();
        foreach (var variable in variables)
            globalsBuilder.AppendLine(variable.GetDeclarationCode());

        var setupBuilder = new StringBuilder();
        foreach (var action in setup)
            setupBuilder.AppendLine(action.GenerateCode());

        var autonomousBuilder = new StringBuilder();
        foreach (var action in autonomous)
            autonomousBuilder.AppendLine(action.GenerateCode());

        var teleopBuilder = new StringBuilder();

        teleopBuilder.AppendLine("  // Always running");
        foreach (var action in teleop.AlwaysRunning)
            teleopBuilder.AppendLine(action.GenerateCode());

        teleopBuilder.AppendLine("  // Button rules");
        foreach (var rule in teleop.ButtonRules)
            teleopBuilder.AppendLine(rule.GenerateCode());
        var functionsBuilder = new StringBuilder();

        foreach (var function in functions)
            functionsBuilder.AppendLine(function.GenerateCode());

        return $@"
#include <TELEOP.h>
#include <PRIZM.h>

PRIZM prizm;
EXPANSION expansion;
EXPANSION expansion2;
PS4 ps4;

// ===== Program variables =====
{globalsBuilder}

bool autoMode = false;

void setup() {{
  prizm.PrizmBegin();
  Serial.begin(115200);

  ps4.setDeadZone(LEFT, 10);
  ps4.setDeadZone(RIGHT, 10);
 // ===== User Setup =====
{setupBuilder}
}}
// ===== User Functions =====
{functionsBuilder}
void loop() {{
  ps4.getPS4();

  if (ps4.Button(OPTIONS)) {{
    autoMode = true;
  }}

  if (autoMode) {{
    RunAutonomous();
  }} else {{
    RunTeleop();
  }}

  delay(20);
}}

void RunAutonomous() {{
{autonomousBuilder}
}}

void RunTeleop() {{
{teleopBuilder}
}}
";
    }

    public string SaveToFile(
        IEnumerable<RobotAction> setup,
        IEnumerable<RobotAction> autonomous,
        TeleopProgram teleop,
        IEnumerable<ProgramVariable> variables,
        IEnumerable<ProgramFunction> functions)
    {
        string code = GenerateCode(setup, autonomous, teleop, variables, functions);

        string folderPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "robot");

        Directory.CreateDirectory(folderPath);

        string filePath = Path.Combine(folderPath, "robot.ino");

        File.WriteAllText(filePath, code);

        return folderPath;
    }
}