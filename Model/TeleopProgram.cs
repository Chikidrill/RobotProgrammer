using System.Collections.ObjectModel;
using Model.RobotActions;

namespace Model;

public class TeleopProgram
{
    public ObservableCollection<RobotAction> AlwaysRunning { get; set; } = new();
    public ObservableCollection<RobotAction> ButtonRules { get; set; } = new();
}