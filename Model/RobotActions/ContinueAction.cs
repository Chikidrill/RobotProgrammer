using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.RobotActions
{
    public class ContinueAction : RobotAction
    {
        public override string ActionType => "Continue";
        public override string DisplayType => "Continue";

        public override string GenerateCode()
            => "continue;\n";
    }
}
