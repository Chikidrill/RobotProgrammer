using RobotProgrammer.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using ViewModel;

namespace RobotProgrammer.ViewModel
{
    public class NewTemplateVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // Результат, который потом добавим в Actions
        public CustomAction Result { get; private set; } = new();

        // Имя шаблона
        private string _templateName = "Новый шаблон";
        public string TemplateName
        {
            get => _templateName;
            set { _templateName = value; OnPropertyChanged(nameof(TemplateName)); }
        }

        // Код шаблона с параметрами вида {SpeedLeft}, {SpeedRight}, {DurationMs}
        private string _templateCode = "";
        public string TemplateCode
        {
            get => _templateCode;
            set { _templateCode = value; OnPropertyChanged(nameof(TemplateCode)); }
        }

        // Коллекция параметров, которые пользователь может задавать
        public Dictionary<string, int> Parameters { get; } = new();

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        public NewTemplateVM()
        {
            OkCommand = new RelayCommand(Ok);
            CancelCommand = new RelayCommand(Cancel);
        }

        public bool? DialogResult { get; private set; }

        private void Ok()
        {
            // Заполняем результат
            Result.TemplateName = TemplateName;
            Result.TemplateCode = TemplateCode;
            Result.Parameters = new Dictionary<string, int>(Parameters);

            DialogResult = true;
        }

        private void Cancel()
        {
            DialogResult = false;
        }
    }
}
