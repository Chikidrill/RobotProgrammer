using RobotProgrammer.Model;      // если модели тут
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

using System.Windows.Input;
using ViewModel;

namespace RobotProgrammer.ViewModel;

public class MainVM: INotifyPropertyChanged
{
    public ObservableCollection<RobotAction> Actions { get; } = new();
    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    public ICommand AddMoveCommand { get; }
    public ICommand AddWaitCommand { get; }

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

    public MainVM()
    {
        AddMoveCommand = new RelayCommand(AddMove);
        AddWaitCommand = new RelayCommand(AddWait);

        CompileCommand = new RelayCommand(Compile);
        UploadCommand = new RelayCommand(Upload);
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
}

