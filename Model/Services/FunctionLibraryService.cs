using System.Text.Json;

namespace Model.Services;

public static class FunctionLibraryService
{
    private static readonly string LibraryFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RobotProgrammer",
        "Functions");

    static FunctionLibraryService()
    {
        Directory.CreateDirectory(LibraryFolder);
    }

    public static List<ProgramFunction> LoadAll()
    {
        var result = new List<ProgramFunction>();

        foreach (var file in Directory.GetFiles(LibraryFolder, "*.rfunc"))
        {
            try
            {
                var json = File.ReadAllText(file);
                var function = JsonSerializer.Deserialize<ProgramFunction>(json);

                if (function != null)
                    result.Add(function);
            }
            catch
            {
                // битый файл библиотеки просто пропускаем
            }
        }

        return result;
    }

    public static void Save(ProgramFunction function)
    {
        Directory.CreateDirectory(LibraryFolder);

        string safeName = MakeSafeFileName(function.Name);
        string path = Path.Combine(LibraryFolder, safeName + ".rfunc");

        string json = JsonSerializer.Serialize(
            function,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        File.WriteAllText(path, json);
    }

    private static string MakeSafeFileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "function";

        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');

        return name;
    }
}