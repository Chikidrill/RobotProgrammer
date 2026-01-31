using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using ViewModel;
using RobotProgrammer.Model;
namespace RobotProgrammer.ViewModel;

public class MainVM: INotifyPropertyChanged
{
    private readonly IFileDialogService _fileDialog;
    private RobotAction _selectedAction;
    private IWindowService _windowService;
    public ObservableCollection<RobotAction> Actions { get; } = new();
    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    public ICommand AddMoveCommand { get; }
    public ICommand AddWaitCommand { get; }
    public ICommand SaveAsProjectCommand { get; }
    public ICommand OpenProjectCommand { get; }
    public ICommand CompileCommand { get; }
    public ICommand UploadCommand { get; }
    public ICommand NewTemplateCommand { get; }
    public ICommand LoadTemplateCommand { get; }
    public ICommand EditTemplateCommand { get; }


    private readonly ArduinoCodeGenerator _generator = new();
    private readonly ArduinoCliService _cli = new();

    // Лог для UI
    public ObservableCollection<string> Log { get; } = new();

    public string LogString => string.Join(Environment.NewLine, Log);

    private void AddLog(string message)
    {
        Log.Add(message);
        OnPropertyChanged(nameof(LogString)); // уведомляем TextBox, что надо обновить
    }

    private string projectPath = "robot"; // путь к скетчу

    public MainVM(IFileDialogService fileDialog, IWindowService windowService)
    {
        _fileDialog = fileDialog;
        _windowService = windowService;
        AddMoveCommand = new RelayCommand(AddMove);
        AddWaitCommand = new RelayCommand(AddWait);
        SaveAsProjectCommand = new RelayCommand(SaveAsProject);
        OpenProjectCommand = new RelayCommand(OpenProject);
        CompileCommand = new RelayCommand(Compile);
        UploadCommand = new RelayCommand(Upload);
        NewTemplateCommand = new RelayCommand(OpenNewTemplateWindow);
        LoadTemplateCommand = new RelayCommand(OpenTemplatePicker);
        EditTemplateCommand = new RelayCommand(EditTemplate);

    }
    private void SaveAsProject()
    {
        var path = _fileDialog.SaveFile(
            "Robot project (*.rproj)|*.rproj",
            ".rproj");

        if (path == null)
            return;

        ProjectFileSaving.Save(path, Actions);
        AddLog($"Проект сохранён: {path}");
    }

    private void OpenProject()
    {
        try
        {
            var path = _fileDialog.OpenFile("Robot project (*.rproj)|*.rproj");

            if (path == null)
                return;

            Actions.Clear();

            var loaded = ProjectFileSaving.Load(path);
            foreach (var action in loaded)
                Actions.Add(action);

            AddLog($"Проект загружен: {path}");
        }
        catch (Exception ex)
        {
            AddLog("[Ошибка загрузки] " + ex.Message);
        }
    }

    private void AddMove()
    {
        Actions.Add(new MoveAction { SpeedLeft = 100, SpeedRight = 100, DurationMs = 1000 });
        AddLog("Добавлено движение");
    }

    private void AddWait()
    {
        Actions.Add(new WaitAction { DurationMs = 500 });
        AddLog("Добавлена пауза");
    }

    private void Compile()
    {
        try
        {
            // Генерируем код
            Directory.CreateDirectory(projectPath);
            string code = _generator.GenerateCode(Actions);
            File.WriteAllText(Path.Combine(projectPath, "robot.ino"), code);
            AddLog("Код сгенерирован");

            // Компиляция
            _cli.Compile(projectPath, line => AddLog(line));
            AddLog("Компиляция завершена");
        }
        catch (Exception ex)
        {
                AddLog("[Ошибка] " + ex.Message);
        }
    }

    private void Upload()
    {
        try
        {
            _cli.Upload(projectPath, "COM3", line => AddLog(line)); // COM порт можно вынести в настройки
            AddLog("Загрузка завершена");
        }
        catch (Exception ex)
        {
            AddLog("[Ошибка] " + ex.Message);
        }
    }
    
    public RobotAction SelectedAction
    {
        get => _selectedAction;
        set
        {
            _selectedAction = value;
            OnPropertyChanged(nameof(SelectedAction));
        }
    }
    private void OpenNewTemplateWindow()
    {
        var templateVM = new NewTemplateVM();
        if (_windowService.ShowDialog(templateVM) == true)
        {
            Actions.Add(templateVM.Result);
            AddLog($"Создан новый шаблон: {templateVM.Result.TemplateName}");
        }
    }
    private void OpenTemplatePicker()
    {
        try
        {
            // Получаем список всех файлов шаблонов через сервис
            var templates = TemplateService.GetAllTemplateFiles();
            if (templates.Count == 0)
            {
                AddLog("Шаблонов нет.");
                return;
            }

            // Используем наш файл-диалог, чтобы выбрать один из шаблонов
            string selectedFile = _fileDialog.OpenFile("JSON шаблоны (*.json)|*.json");
            if (selectedFile == null)
                return;

            // Загружаем шаблон по имени файла
            var template = TemplateService.LoadTemplate(Path.GetFileName(selectedFile));
            Actions.Add(template);
            AddLog($"Загружен шаблон: {template.TemplateName}");
        }
        catch (Exception ex)
        {
            AddLog("[Ошибка загрузки шаблона] " + ex.Message);
        }
    }

    private void EditTemplate()
    {
        if (SelectedAction is not CustomAction template)
        {
            AddLog("Выберите шаблон для редактирования.");
            return;
        }

        var vm = new NewTemplateVM
        {
            TemplateName = template.TemplateName,
            TemplateCode = template.TemplateCode
        };

        // Копируем параметры
        vm.Parameters.Clear();
        foreach (var p in template.Parameters)
            vm.Parameters.Add(new ParameterItem { Name = p.Name, Value = p.Value });

        if (_windowService.ShowDialog(vm) == true)
        {
            // Обновляем шаблон
            template.TemplateName = vm.TemplateName;
            template.TemplateCode = vm.TemplateCode;

            template.Parameters.Clear();
            foreach (var p in vm.Parameters)
                template.Parameters.Add(new ParameterItem { Name = p.Name, Value = p.Value });

            TemplateService.SaveTemplate(template);

            AddLog($"Шаблон '{template.TemplateName}' обновлён");
        }
    }



}

