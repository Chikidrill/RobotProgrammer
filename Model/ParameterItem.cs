using System.ComponentModel;

public class ParameterItem : INotifyPropertyChanged
{
    public string Name { get; set; } = "";

    private string _value = "0"; // строка для TextBox
    public string Value
    {
        get => _value;
        set
        {
            if (_value != value)
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }
    }

    // Получить int для кода
    public int IntValue => int.TryParse(_value, out var v) ? v : 0;

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(string name)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
