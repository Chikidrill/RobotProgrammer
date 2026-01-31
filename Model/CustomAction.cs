using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RobotProgrammer.Model
{
    public class CustomAction : RobotAction
    {
        public string TemplateName { get; set; } = "Новый шаблон";

        // Код шаблона с {Placeholders} для параметров
        public string TemplateCode { get; set; } = "";

        // Параметры, которые пользователь может задавать
        public ObservableCollection<ParameterItem> Parameters { get; set; } = new();

        public override string DisplayType => TemplateName;

        // Генерация кода для Arduino
        public override string GenerateCode()
        {
            string code = TemplateCode;
            foreach (var param in Parameters)
                code = code.Replace($"{{{param.Name}}}", param.IntValue.ToString());
            return code;
        }

    }
}
