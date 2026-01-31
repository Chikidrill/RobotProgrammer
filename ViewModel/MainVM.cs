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

    public MainVM(IFileDialogService fileDialog)
    {
        _fileDialog = fileDialog;
        AddMoveCommand = new RelayCommand(AddMove);
        AddWaitCommand = new RelayCommand(AddWait);
        SaveAsProjectCommand = new RelayCommand(SaveAsProject);
        OpenProjectCommand = new RelayCommand(OpenProject);
        CompileCommand = new RelayCommand(Compile);
        UploadCommand = new RelayCommand(Upload);
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
}

