using Model.RobotActions;
using Model.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using ViewModel;

namespace RobotProgrammer.ViewModel
{
    public class NewTemplateVM : INotifyPropertyChanged
    {
        private ParameterItem _selectedParameter;
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public event EventHandler<bool> RequestClose;

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

        public ObservableCollection<ParameterItem> Parameters { get; } = new();

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand AddParameterCommand { get; }
        public ICommand RemoveParameterCommand { get; }

        public NewTemplateVM()
        {
            OkCommand = new RelayCommand(Ok);
            CancelCommand = new RelayCommand(Cancel);
            AddParameterCommand = new RelayCommand(() =>
            {
                Parameters.Add(new ParameterItem { Name = "NewParam", Value = "0" });
            });
            RemoveParameterCommand = new RelayCommand(() =>
            {
                if (SelectedParameter != null)
                    Parameters.Remove(SelectedParameter);
            });
        }
       
        public ParameterItem SelectedParameter
        {
            get => _selectedParameter;
            set { _selectedParameter = value; OnPropertyChanged(nameof(SelectedParameter)); }
        }

        private void Ok()
        {
            Result.TemplateName = TemplateName;
            Result.TemplateCode = TemplateCode;
            Result.Parameters = new ObservableCollection<ParameterItem>(Parameters);
            TemplateService.SaveTemplate(Result);
            RequestClose?.Invoke(this, true);
        }

        private void Cancel()
        {
            RequestClose?.Invoke(this, false);
        }
    }
}
