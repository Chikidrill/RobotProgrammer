namespace RobotProgrammer.Model;
public abstract class RobotAction
{
    public abstract string GenerateCode();
    public abstract string ActionType { get; }

    public int SpeedLeft { get; set; }
    public int SpeedRight { get; set; }
    public int DurationMs { get; set; }
}
