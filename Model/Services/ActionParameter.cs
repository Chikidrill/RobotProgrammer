using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Model.Services
{
    public class ActionParameter : INotifyPropertyChanged
    {
        public string Name { get; set; } = "";

        private int _value;
        public int Value
        {
            get => _value;
            set
            {
                _value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }
        [JsonIgnore]
        public string TextValue
        {
            get => _value.ToString();
            set
            {
                if (int.TryParse(value, out int v))
                    _value = v;
                OnPropertyChanged(nameof(TextValue));
                OnPropertyChanged(nameof(Value));
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}