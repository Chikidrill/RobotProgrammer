using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace RobotProgrammer.Model;

public abstract class RobotAction
{
    [JsonPropertyName("ActionType")]
    public abstract string ActionType { get; }
    [JsonIgnore]
    public virtual string DisplayType => GetType().Name.Replace("Action", "");
   
    public virtual ObservableCollection<ActionParameter> GetParameters()
    {
        return new ObservableCollection<ActionParameter>();
    }

    public virtual void ApplyParameters(IEnumerable<ActionParameter> parameters) { }
    public abstract string GenerateCode();
}
