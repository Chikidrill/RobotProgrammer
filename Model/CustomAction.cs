using System.Collections.Generic;

namespace RobotProgrammer.Model
{
    public class CustomAction : RobotAction
    {
        public string TemplateName { get; set; } = "Новый шаблон";

        // Параметры действия: ключ = имя, значение = текущее значение
        public Dictionary<string, int> Parameters { get; set; } = new();

        // Тело кода с плейсхолдерами {ParamName}
        public string TemplateCode { get; set; } = "";

        // Для отображения типа в таблице
        public override string DisplayType => TemplateName;

        // Метод генерации кода для Arduino
        public string GenerateCode()
        {
            string code = TemplateCode;
            foreach (var kv in Parameters)
            {
                code = code.Replace("{" + kv.Key + "}", kv.Value.ToString());
            }
            return code;
        }
    }
}
