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

    public class NewTemplateViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public CustomAction Result { get; private set; } = new();

        private string _templateName = "Новый шаблон";
        public string TemplateName
        {
            get => _templateName;
            set { _templateName = value; OnPropertyChanged(nameof(TemplateName)); }
        }

        private string _templateCode = "";
        public string TemplateCode
        {
            get => _templateCode;
            set { _templateCode = value; OnPropertyChanged(nameof(TemplateCode)); }
        }

        public ObservableCollection<ParameterEntry> Parameters { get; } = new();

        public ICommand AddParameterCommand { get; }
        public ICommand RemoveParameterCommand { get; }
        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        public bool? DialogResult { get; private set; }

        public NewTemplateViewModel()
        {
            AddParameterCommand = new RelayCommand(AddParameter);
            RemoveParameterCommand = new RelayCommand(RemoveParameter);
            OkCommand = new RelayCommand(Ok);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void AddParameter()
        {
            Parameters.Add(new ParameterEntry { Name = "param", Value = 0 });
        }

        private void RemoveParameter()
        {
            if (Parameters.Count > 0)
                Parameters.RemoveAt(Parameters.Count - 1);
        }

        private void Ok()
        {
            Result.TemplateName = TemplateName;
            Result.TemplateCode = TemplateCode;
            Result.Parameters.Clear();

            foreach (var p in Parameters)
                Result.Parameters[p.Name] = p.Value;

            DialogResult = true;
        }

        private void Cancel()
        {
            DialogResult = false;
        }
    }
}
