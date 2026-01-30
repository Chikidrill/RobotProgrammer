using System.IO;
namespace RobotProgrammer.Model;
public class ArduinoCodeGenerator
{
    public string GenerateCode(IEnumerable<RobotAction> actions)
    {
        var actionsCode = string.Join("\n",
            actions.Select(a => a.GenerateCode()));

        return $@"
#include <PRIZM.h>


PRIZM prizm;


void setup() {{
  prizm.PrizmBegin();
}}

void loop() {{
{actionsCode}
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

        return folderPath; // важно для arduino-cli
    }
}
