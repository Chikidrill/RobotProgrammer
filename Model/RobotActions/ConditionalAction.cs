using Model.Services;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Model.RobotActions;

public class ConditionalAction : ContainerAction
{
    public ConditionalAction()
    {
        EnsureBranches();
    }

    public string Condition { get; set; } = "true";

    public override string ActionType => "ConditionalAction";

    [JsonIgnore]
    public override string DisplayType => $"Если [{Condition}]";

    [JsonIgnore]
    public BranchAction IfBranch
    {
        get
        {
            EnsureBranches();
            return GetOrCreateBranch("IF");
        }
    }

    [JsonIgnore]
    public BranchAction ElseBranch
    {
        get
        {
            EnsureBranches();
            return GetOrCreateBranch("ELSE");
        }
    }

    private BranchAction GetOrCreateBranch(string name)
    {
        var branch = Children
            .OfType<BranchAction>()
            .FirstOrDefault(x => x.BranchName == name);

        if (branch != null)
            return branch;

        branch = new BranchAction(name)
        {
            Parent = this
        };

        Children.Add(branch);
        return branch;
    }

    private void EnsureBranches()
    {
        GetOrCreateBranch("IF");
        GetOrCreateBranch("ELSE");

        foreach (var child in Children)
            child.Parent = this;
    }

    public override string GenerateCode()
    {
        EnsureBranches();

        string code = $"if ({Condition}) {{\n";

        foreach (var child in IfBranch.Children)
            code += child.GenerateCode() + "\n";

        code += "}";

        if (ElseBranch.Children.Count > 0)
        {
            code += " else {\n";

            foreach (var child in ElseBranch.Children)
                code += child.GenerateCode() + "\n";

            code += "}";
        }

        code += "\n";
        return code;
    }

    public override ObservableCollection<ActionParameter> GetParameters()
    {
        return new ObservableCollection<ActionParameter>
        {
            new ActionParameter
            {
                Name = "Условие",
                TextValue = Condition
            }
        };
    }

    public override void ApplyParameters(IEnumerable<ActionParameter> parameters)
    {
        foreach (var p in parameters)
        {
            if (p.Name == "Условие")
                Condition = p.TextValue;
        }
    }
}