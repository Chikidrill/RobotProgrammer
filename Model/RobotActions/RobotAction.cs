using Model.Services;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Model.RobotActions;

public abstract class RobotAction
{
    public virtual bool IsContainer => false;
    [JsonPropertyName("ActionType")]
    public abstract string ActionType { get; }
    [JsonIgnore]
    public virtual string DisplayType => GetType().Name.Replace("Action", "");
   
    public virtual ObservableCollection<ActionParameter> GetParameters()
    {
        return new ObservableCollection<ActionParameter>();
    }
    [JsonIgnore]
    public ContainerAction? Parent { get; set; }
    public virtual void ApplyParameters(IEnumerable<ActionParameter> parameters) { }
    public abstract string GenerateCode();
}
