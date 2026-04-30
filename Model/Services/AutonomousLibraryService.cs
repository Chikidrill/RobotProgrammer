using Model.RobotActions;
using System.Text.Json;

namespace Model.Services;

public static class AutonomousLibraryService
{
    private static readonly string LibraryFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RobotProgrammer",
        "Autonomous");

    static AutonomousLibraryService()
    {
        Directory.CreateDirectory(LibraryFolder);
    }

    public static List<AutonomousRoutine> LoadAll()
    {
        var result = new List<AutonomousRoutine>();

        foreach (var file in Directory.GetFiles(LibraryFolder, "*.rauto"))
        {
            try
            {
                var json = File.ReadAllText(file);
                var routine = JsonSerializer.Deserialize<AutonomousRoutine>(
                    json,
                    GetOptions());

                if (routine != null)
                    result.Add(routine);
            }
            catch
            {
                // битую автономку пропускаем
            }
        }

        return result
            .OrderBy(x => x.Name)
            .ToList();
    }

    public static void Save(AutonomousRoutine routine)
    {
        Directory.CreateDirectory(LibraryFolder);

        var fileName = MakeSafeFileName(routine.Name) + ".rauto";
        var path = Path.Combine(LibraryFolder, fileName);

        var json = JsonSerializer.Serialize(
            routine,
            GetOptions());

        File.WriteAllText(path, json);
    }

    private static JsonSerializerOptions GetOptions()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        options.Converters.Add(new RobotActionConverter());

        return options;
    }

    private static string MakeSafeFileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "autonomous";

        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');

        return name.Trim();
    }
}