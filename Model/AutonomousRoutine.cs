using Model.RobotActions;
using System.Collections.ObjectModel;

namespace Model;

public class AutonomousRoutine
{
    public string Name { get; set; } = "Новая автономка";

    public ObservableCollection<RobotAction> Actions { get; set; } = new();

    public override string ToString()
    {
        return Name;
    }
}