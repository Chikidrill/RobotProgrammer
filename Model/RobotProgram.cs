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
        public ObservableCollection<ProgramInclude> Includes { get; set; } = new()
    {
        new ProgramInclude
        {
            Header = "TELEOP.h",
            Kind = ProgramIncludeKind.System,
            IsEnabled = true
        },

        new ProgramInclude
        {
            Header = "PRIZM.h",
            Kind = ProgramIncludeKind.System,
            IsEnabled = true
        }
    };

        public string GlobalCode { get; set; } =
    @"PRIZM prizm;
EXPANSION expansion;
EXPANSION expansion2;
PS4 ps4;

bool autoMode = false;";

        public string SetupBaseCode { get; set; } =
    @"prizm.PrizmBegin();
Serial.begin(115200);

ps4.setDeadZone(LEFT, 10);
ps4.setDeadZone(RIGHT, 10);";

        public string LoopBaseCode { get; set; } =
    @"ps4.getPS4();

if (ps4.Button(OPTIONS)) {
  autoMode = true;
}";

        public string LoopEndCode { get; set; } =
    @"delay(20);";

        public ObservableCollection<ProgramVariable> Variables { get; } = new();
        public ObservableCollection<RobotAction> Setup { get; } = new();
        public ObservableCollection<RobotAction> Autonomous { get; } = new();
        public TeleopProgram Teleop { get; } = new();
        public ObservableCollection<ProgramFunction> Functions { get;} = new();
    }
}
