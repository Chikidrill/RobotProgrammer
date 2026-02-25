using System.Text;
using Model.RobotActions;
using System.IO;

namespace Model.ArduinoServices;

public class ArduinoCodeGenerator
{
    public string GenerateCode(IEnumerable<RobotAction> actions)
    {
        var builder = new StringBuilder();

        foreach (var action in actions)
            builder.Append(action.GenerateCode());

        return $@"
#include <PRIZM.h>

PRIZM prizm;

void setup() {{
  prizm.PrizmBegin();
}}

void loop() {{
{builder}
  while(true);
}}
";
    }

    public string SaveToFile(IEnumerable<RobotAction> actions)
    {
        string code = GenerateCode(actions);

        string folderPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "robot");

        Directory.CreateDirectory(folderPath);

        string filePath = Path.Combine(folderPath, "robot.ino");

        File.WriteAllText(filePath, code);

        return folderPath;
    }
}