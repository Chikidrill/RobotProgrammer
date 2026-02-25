using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.RobotActions
{
    public class BreakAction : RobotAction
    {
        public override string ActionType => "Break";
        public override string DisplayType => "Break";

        public override string GenerateCode()
            => "break;\n";
    }
}
