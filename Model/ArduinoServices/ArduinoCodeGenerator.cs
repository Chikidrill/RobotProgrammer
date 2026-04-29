using System.Text;
using Model.RobotActions;
using System.IO;

namespace Model.ArduinoServices;

public class ArduinoCodeGenerator
{
    public string GenerateCode(IEnumerable<RobotAction> autonomous, IEnumerable<RobotAction> teleop)
    {
        var autoBuilder = new StringBuilder();
        foreach (var action in autonomous)
            autoBuilder.Append(action.GenerateCode());

        var teleopBuilder = new StringBuilder();
        foreach (var action in teleop)
            teleopBuilder.Append(action.GenerateCode());

        return $@"
#include <PRIZM.h>

PRIZM prizm;

void setup() {{
    prizm.PrizmBegin();
}}

void loop() {{
    if (autoMode) {{
        RunAutonomous();
    }} else {{
        RunTeleop();
    }}
}}

void RunAutonomous() {{
{autoBuilder}
}}

void RunTeleop() {{
{teleopBuilder}
}}
";
    }

    public string SaveToFile(IEnumerable<RobotAction> autonomous, IEnumerable<RobotAction> teleop)
    {
        string code = GenerateCode(autonomous, teleop);

        string folderPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "robot");

        Directory.CreateDirectory(folderPath);

        string filePath = Path.Combine(folderPath, "robot.ino");

        File.WriteAllText(filePath, code);

        return folderPath;
    }
}