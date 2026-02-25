using Model.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Model.RobotActions
{
    public class CustomAction : RobotAction
    {
        public CustomAction()
        {
            Parameters = new ObservableCollection<ParameterItem>();
        }
        public override string ActionType => "CustomAction";
        public string TemplateName { get; set; } = "Новый шаблон";

        // Код шаблона с {Placeholders} для параметров
        public string TemplateCode { get; set; } = "";

        // Параметры, которые пользователь может задавать
        public ObservableCollection<ParameterItem> Parameters { get; set; } = new();
        public override ObservableCollection<ActionParameter> GetParameters()
       => new ObservableCollection<ActionParameter>(Parameters.Select(p => new ActionParameter { Name = p.Name, Value = p.IntValue }));

        public override string DisplayType => TemplateName;

        // Генерация кода для Arduino
        public override string GenerateCode()
        {
            string code = TemplateCode;
            foreach (var param in Parameters)
                code = code.Replace($"{{{param.Name}}}", param.IntValue.ToString());
            return code;
        }

        public override void ApplyParameters(IEnumerable<ActionParameter> parameters)
        {
            foreach (var p in parameters)
            {
                var target = Parameters.FirstOrDefault(x => x.Name == p.Name);
                if (target != null) target.IntValue = p.Value;
            }
        }
    }
}
