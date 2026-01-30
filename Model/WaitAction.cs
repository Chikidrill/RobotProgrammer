namespace RobotProgrammer.Model;
public class WaitAction : RobotAction
{


    public override string ActionType => "Wait";

    public override string GenerateCode()
    {
        return $"  delay({DurationMs});";
    }
}

