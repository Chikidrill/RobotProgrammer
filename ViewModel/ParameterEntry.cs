using RobotProgrammer.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using ViewModel;

namespace RobotProgrammer.ViewModel
{
    public class ParameterEntry : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private string _name;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        private int _value;
        public int Value
        {
            get => _value;
            set { _value = value; OnPropertyChanged(nameof(Value)); }
        }
    }
}
