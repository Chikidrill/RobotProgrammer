using Model.RobotActions;
using RobotProgrammer.Model;
using System.Text.Json;

namespace Model.Services;

public static class ProjectFileSaving
{
    private static JsonSerializerOptions GetOptions()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        options.Converters.Add(new RobotActionConverter());
        return options;
    }



    public static void Save(string filePath, IEnumerable<RobotAction> actions)
    {
        var json = JsonSerializer.Serialize(actions, GetOptions());
        File.WriteAllText(filePath, json);
    }

    public static List<RobotAction> Load(string filePath)
    {
        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<List<RobotAction>>(json, GetOptions())!;
    }
}
