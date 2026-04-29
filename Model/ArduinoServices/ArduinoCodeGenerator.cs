using System.Text;
using Model.RobotActions;

namespace Model.ArduinoServices;

public class ArduinoCodeGenerator
{
    public string GenerateCode(
        IEnumerable<RobotAction> setup,
        IEnumerable<RobotAction> autonomous,
        IEnumerable<RobotAction> teleop,
        IEnumerable<ProgramVariable> variables)
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
        foreach (var action in teleop)
            teleopBuilder.AppendLine(action.GenerateCode());

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
        IEnumerable<RobotAction> teleop,
        IEnumerable<ProgramVariable> variables)
    {
        string code = GenerateCode(setup, autonomous, teleop, variables);

        string folderPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "robot");

        Directory.CreateDirectory(folderPath);

        string filePath = Path.Combine(folderPath, "robot.ino");

        File.WriteAllText(filePath, code);

        return folderPath;
    }
}