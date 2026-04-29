using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Model;

public class FunctionParameter : INotifyPropertyChanged
{
    private string _type = "int";
    private string _name = "value";

    public string Type
    {
        get => _type;
        set
        {
            _type = value;
            OnPropertyChanged(nameof(Type));
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(SafeName));
        }
    }

    public string SafeName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Name))
                return "value";

            var cleaned = Regex.Replace(Name.Trim(), @"[^a-zA-Z0-9_]", "_");

            if (char.IsDigit(cleaned[0]))
                cleaned = "_" + cleaned;

            return cleaned;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string name)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}