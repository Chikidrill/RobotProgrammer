using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace RobotProgrammer.Model
{
    public static class TemplateService
    {
        private static readonly string TemplatesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");

        static TemplateService()
        {
            Directory.CreateDirectory(TemplatesFolder);
        }

        public static void SaveTemplate(CustomAction template)
        {
            string path = Path.Combine(TemplatesFolder, template.TemplateName + ".json");
            string json = JsonSerializer.Serialize(template, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        public static CustomAction LoadTemplate(string filename)
        {
            string path = Path.Combine(TemplatesFolder, filename);
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<CustomAction>(json);
        }

        public static List<string> GetAllTemplateFiles()
        {
            return new List<string>(Directory.GetFiles(TemplatesFolder, "*.json", SearchOption.TopDirectoryOnly));
        }
    }
}
