using System.Collections.Generic;

namespace RobotProgrammer.Model
{
    public class CustomAction : RobotAction
    {
        public string TemplateName { get; set; } = "Новый шаблон";

        // Код шаблона с {Placeholders} для параметров
        public string TemplateCode { get; set; } = "";

        // Параметры, которые пользователь может задавать
        public Dictionary<string, int> Parameters { get; set; } = new();

        public override string DisplayType => TemplateName;

        // Генерация кода для Arduino
        public override string GenerateCode()
        {
            string code = TemplateCode;

            foreach (var kv in Parameters)
            {
                // заменяем {ParameterName} на значение
                code = code.Replace("{" + kv.Key + "}", kv.Value.ToString());
            }

            return code;
        }
    }
}
