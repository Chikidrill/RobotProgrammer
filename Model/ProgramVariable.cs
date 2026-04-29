using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Model;

public class ProgramVariable : INotifyPropertyChanged
{
    private ProgramVariableType _type = ProgramVariableType.Int;
    private string _name = "MY_VAR";
    private string _defaultValue = "0";
    private bool _isConstant = true;

    public ProgramVariableType Type
    {
        get => _type;
        set
        {
            if (_type != value)
            {
                _type = value;
                OnPropertyChanged(nameof(Type));
                NormalizeDefaultValue();
            }
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    public string DefaultValue
    {
        get => _defaultValue;
        set
        {
            _defaultValue = value;
            OnPropertyChanged(nameof(DefaultValue));
        }
    }

    public bool IsConstant
    {
        get => _isConstant;
        set
        {
            _isConstant = value;
            OnPropertyChanged(nameof(IsConstant));
        }
    }

    [JsonIgnore]
    public string CppType => Type switch
    {
        ProgramVariableType.Bool => "bool",
        ProgramVariableType.Int => "int",
        ProgramVariableType.Long => "long",
        ProgramVariableType.Float => "float",
        ProgramVariableType.Double => "double",
        ProgramVariableType.String => "String",
        _ => "int"
    };

    [JsonIgnore]
    public string SafeName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Name))
                return "UNNAMED_VAR";

            var cleaned = Regex.Replace(Name.Trim(), @"[^a-zA-Z0-9_]", "_");

            if (char.IsDigit(cleaned[0]))
                cleaned = "_" + cleaned;

            return cleaned;
        }
    }

    public string GetDeclarationCode()
    {
        string prefix = IsConstant ? "const " : "";
        return $"{prefix}{CppType} {SafeName} = {FormatValueForCpp()};";
    }

    private string FormatValueForCpp()
    {
        var value = DefaultValue?.Trim() ?? "";

        return Type switch
        {
            ProgramVariableType.Bool => FormatBool(value),
            ProgramVariableType.Int => FormatInteger(value, "0"),
            ProgramVariableType.Long => FormatInteger(value, "0"),
            ProgramVariableType.Float => FormatFloat(value),
            ProgramVariableType.Double => FormatFloat(value),
            ProgramVariableType.String => $"\"{EscapeString(value)}\"",
            _ => "0"
        };
    }

    private static string FormatBool(string value)
    {
        value = value.ToLowerInvariant();

        if (value is "true" or "1" or "yes" or "да")
            return "true";

        return "false";
    }

    private static string FormatInteger(string value, string fallback)
    {
        return long.TryParse(value, out var parsed)
            ? parsed.ToString()
            : fallback;
    }

    private static string FormatFloat(string value)
    {
        value = value.Replace(',', '.');

        return double.TryParse(
            value,
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out var parsed)
            ? parsed.ToString(System.Globalization.CultureInfo.InvariantCulture)
            : "0.0";
    }

    private static string EscapeString(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"");
    }

    private void NormalizeDefaultValue()
    {
        DefaultValue = Type switch
        {
            ProgramVariableType.Bool => "false",
            ProgramVariableType.Int => "0",
            ProgramVariableType.Long => "0",
            ProgramVariableType.Float => "0.0",
            ProgramVariableType.Double => "0.0",
            ProgramVariableType.String => "",
            _ => "0"
        };
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string name)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}