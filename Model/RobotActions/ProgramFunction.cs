using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Model;

public class ProgramFunction : INotifyPropertyChanged
{
    private string _name = "myFunction";
    private string _returnType = "void";
    private string _bodyCode = "// function body";

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(DisplayName));
        }
    }

    public string ReturnType
    {
        get => _returnType;
        set
        {
            _returnType = value;
            OnPropertyChanged(nameof(ReturnType));
        }
    }

    public ObservableCollection<FunctionParameter> Parameters { get; set; } = new();

    public string BodyCode
    {
        get => _bodyCode;
        set
        {
            _bodyCode = value;
            OnPropertyChanged(nameof(BodyCode));
        }
    }

    public string DisplayName => GetSignatureCode();

    public string SafeName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Name))
                return "unnamedFunction";

            var cleaned = Regex.Replace(Name.Trim(), @"[^a-zA-Z0-9_]", "_");

            if (char.IsDigit(cleaned[0]))
                cleaned = "_" + cleaned;

            return cleaned;
        }
    }

    public string GetSignatureCode()
    {
        var parameters = string.Join(", ", Parameters.Select(p => $"{p.Type} {p.Name}"));
        return $"{ReturnType} {SafeName}({parameters})";
    }

    public string GenerateCode()
    {
        return $@"
{GetSignatureCode()} {{
{BodyCode}
}}
";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string name)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}