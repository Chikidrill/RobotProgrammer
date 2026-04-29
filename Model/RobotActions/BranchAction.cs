using System.Text.Json.Serialization;

namespace Model.RobotActions;

public class BranchAction : ContainerAction
{
    public BranchAction()
    {
    }

    public BranchAction(string branchName)
    {
        BranchName = branchName;
    }

    public string BranchName { get; set; } = "IF";

    public override string ActionType => "BranchAction";

    [JsonIgnore]
    public override string DisplayType => BranchName;

    public override string GenerateCode()
    {
        string code = "";

        foreach (var child in Children)
            code += child.GenerateCode() + "\n";

        return code;
    }
}