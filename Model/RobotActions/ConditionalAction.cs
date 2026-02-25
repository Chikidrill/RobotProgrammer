using Model.RobotActions;
using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
namespace Model.RobotActions;

public class ConditionalAction : ContainerAction
{
    public override string ActionType => "If";
    public string Condition { get; set; } = "true";

    public ObservableCollection<RobotAction> TrueBranch { get; set; }
        = new();

    public ObservableCollection<RobotAction> FalseBranch { get; set; }
        = new();

    public override bool IsContainer => true;

    public override string DisplayType => $"Если [{Condition}]";

    public override string GenerateCode()
    {
        string code = $"{{\nif({Condition}){{\n";

        foreach (var a in TrueBranch)
            code += a.GenerateCode();

        code += "}";

        if (FalseBranch.Any())
        {
            code += "else{\n";
            foreach (var a in FalseBranch)
                code += a.GenerateCode();
            code += "}";
        }

        code += "\n";
        return code;
    }
    public ConditionalAction(){}
}