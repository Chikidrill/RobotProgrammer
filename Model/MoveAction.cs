namespace RobotProgrammer.Model;

public class MoveAction : RobotAction
{
    public override string GenerateCode()
    {
        return $@"
prizm.setMotorSpeeds({SpeedLeft}, {SpeedRight});
delay({DurationMs});
prizm.setMotorSpeeds(0,0);";
    }
}
