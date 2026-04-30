using System.ComponentModel;

namespace Model;

public enum ProgramIncludeKind
{
    System,
    Local
}

public class ProgramInclude : INotifyPropertyChanged
{
    private bool _isEnabled = true;
    private string _header = "PRIZM.h";
    private ProgramIncludeKind _kind = ProgramIncludeKind.System;

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled == value)
                return;

            _isEnabled = value;
            OnPropertyChanged(nameof(IsEnabled));
            OnPropertyChanged(nameof(Code));
        }
    }

    public string Header
    {
        get => _header;
        set
        {
            if (_header == value)
                return;

            _header = value;
            OnPropertyChanged(nameof(Header));
            OnPropertyChanged(nameof(Code));
        }
    }

    public ProgramIncludeKind Kind
    {
        get => _kind;
        set
        {
            if (_kind == value)
                return;

            _kind = value;
            OnPropertyChanged(nameof(Kind));
            OnPropertyChanged(nameof(Code));
        }
    }

    public string Code => GenerateCode();

    public string GenerateCode()
    {
        if (string.IsNullOrWhiteSpace(Header))
            return string.Empty;

        var cleanHeader = Header.Trim();

        return Kind switch
        {
            ProgramIncludeKind.Local => $"#include \"{cleanHeader}\"",
            _ => $"#include <{cleanHeader}>"
        };
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}