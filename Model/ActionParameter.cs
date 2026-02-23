using System.ComponentModel;

namespace RobotProgrammer.Model
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

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}