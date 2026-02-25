using Model.RobotActions;
using System.Collections.ObjectModel;

namespace Model.RobotActions;
public abstract class ContainerAction : RobotAction
{
    public ObservableCollection<RobotAction> Children { get; set; }
        = new ObservableCollection<RobotAction>();

    public override bool IsContainer => true;

    public bool Contains(RobotAction action)
    {
        if (Children.Contains(action))
            return true;

        foreach (var child in Children.OfType<ContainerAction>())
            if (child.Contains(action))
                return true;

        return false;
    }
}