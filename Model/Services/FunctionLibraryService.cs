using Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Model.Services;

public static class FunctionLibraryService
{
    private const string Extension = ".json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static List<ProgramFunction> LoadAll()
    {
        AppPaths.EnsureCreated();

        var result = new List<ProgramFunction>();

        foreach (var file in Directory.GetFiles(AppPaths.FunctionLibrary, "*" + Extension))
        {
            try
            {
                var json = File.ReadAllText(file);
                var function = JsonSerializer.Deserialize<ProgramFunction>(json, JsonOptions);

                if (function != null)
                    result.Add(function);
            }
            catch
            {
                // Битый файл библиотеки не должен ломать запуск приложения.
            }
        }

        return result
            .OrderBy(function => function.Name)
            .ToList();
    }

    public static void Save(ProgramFunction function)
    {
        if (function == null)
            throw new ArgumentNullException(nameof(function));

        AppPaths.EnsureCreated();

        var path = GetFunctionPath(function);
        var json = JsonSerializer.Serialize(function, JsonOptions);

        File.WriteAllText(path, json);
    }

    public static bool Delete(ProgramFunction function)
    {
        if (function == null)
            return false;

        AppPaths.EnsureCreated();

        var path = GetFunctionPath(function);

        if (File.Exists(path))
        {
            File.Delete(path);
            return true;
        }

        var safeName = ToSafeFileName(
            string.IsNullOrWhiteSpace(function.SafeName)
                ? function.Name
                : function.SafeName);

        var candidates = Directory
            .GetFiles(AppPaths.FunctionLibrary, "*" + Extension)
            .Where(file =>
                string.Equals(
                    Path.GetFileNameWithoutExtension(file),
                    safeName,
                    StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var candidate in candidates)
            File.Delete(candidate);

        return candidates.Count > 0;
    }

    private static string GetFunctionPath(ProgramFunction function)
    {
        var fileName = ToSafeFileName(
            string.IsNullOrWhiteSpace(function.SafeName)
                ? function.Name
                : function.SafeName);

        return Path.Combine(AppPaths.FunctionLibrary, fileName + Extension);
    }

    private static string ToSafeFileName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "function";

        var invalidChars = Path.GetInvalidFileNameChars();

        var safe = new string(value
            .Select(ch => invalidChars.Contains(ch) ? '_' : ch)
            .ToArray());

        return string.IsNullOrWhiteSpace(safe)
            ? "function"
            : safe;
    }
}