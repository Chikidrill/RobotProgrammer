using System.Text.Json;

namespace RobotProgrammer.Model;

public static class ProjectFileSaving
{
    public static void Save(string filePath, IEnumerable<RobotAction> actions)
    {
        var json = JsonSerializer.Serialize(actions, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(filePath, json);
    }

    public static List<RobotAction> Load(string filePath)
    {
        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<List<RobotAction>>(json);
    }
}
