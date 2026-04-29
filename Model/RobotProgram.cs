using Model.RobotActions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class RobotProgram
    {
        public ObservableCollection<RobotAction> Variables { get; } = new();
        public ObservableCollection<RobotAction> Setup { get; } = new();
        public ObservableCollection<RobotAction> Autonomous { get; } = new();
        public ObservableCollection<RobotAction> Teleop { get; } = new();
    }
}
