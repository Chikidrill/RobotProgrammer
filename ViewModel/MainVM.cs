using Microsoft.Win32;
using Model.ArduinoServices;
using Model.RobotActions;
using Model.Services;
using Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Input;
using ViewModel;
namespace RobotProgrammer.ViewModel;

public class MainVM: INotifyPropertyChanged
{
    private readonly IFileDialogService _fileDialog;
    private RobotAction? _selectedAction;
    private IWindowService _windowService;
    private string _previewCode;
    public ObservableCollection<RobotAction> Actions { get; } = new();
    public event PropertyChangedEventHandler PropertyChanged;
    private readonly IDialogService _dialogService;
    public ObservableCollection<ActionParameter> SelectedParameters =>
    SelectedAction?.GetParameters() ?? new();
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
    public ICommand AddLoopCommand { get; }
    public ICommand AddConditionalCommand { get; }
    public ICommand DeleteSelectedCommand { get; }
    public ICommand CopySelectedCommand { get; }
    public ICommand PasteSelectedCommand { get; }
    private readonly ArduinoCodeGenerator _generator = new();
    private readonly ArduinoCliService _cli = new();

    // Лог для UI
    public ObservableCollection<string> Log { get; } = new();
    private RobotAction _clipboard;
    public string LogString => string.Join(Environment.NewLine, Log);

    private void AddLog(string message)
    {
        Log.Add(message);
        OnPropertyChanged(nameof(LogString)); // уведомляем TextBox, что надо обновить
    }

    private string projectPath = "robot"; // путь к скетчу

    public MainVM(IFileDialogService fileDialog, IWindowService windowService, IDialogService dialogService)
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
        AddLoopCommand = new RelayCommand(AddLoop);
        AddConditionalCommand = new RelayCommand(AddConditional);
        Actions = new ObservableCollection<RobotAction>();
        DeleteSelectedCommand = new RelayCommand(DeleteSelected);
        CopySelectedCommand = new RelayCommand(CopySelected);
        PasteSelectedCommand = new RelayCommand(PasteSelected);
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
            UpdatePreview();
        }
        catch (Exception ex)
        {
            AddLog("[Ошибка загрузки] " + ex.Message);
        }
    }

    private void AddMove()
    {
        Actions.Add(new MoveAction());
        AddLog("Добавлено движение");
        UpdatePreview();
    }

    private void AddWait()
    {
        Actions.Add(new WaitAction());
        AddLog("Добавлена пауза");
        UpdatePreview();
    }

    private void Compile()
    {
        try
        {
            Directory.CreateDirectory(projectPath);
            string code = _generator.GenerateCode(Actions);
            File.WriteAllText(Path.Combine(projectPath, "robot.ino"), code);
            AddLog("Код сгенерирован");

            try
            {
                _cli.Compile(projectPath, line => AddLog(line));
                AddLog("Компиляция завершена");
            }
            catch (Exception ex)
            {
                AddLog("[Ошибка компиляции] " + ex.Message);
                File.WriteAllText(Path.Combine(projectPath, "compile_errors.log"), ex.ToString());
            }
        }
        catch (Exception ex)
        {
            AddLog("[Ошибка] " + ex.Message);
            File.WriteAllText(Path.Combine(projectPath, "compile_errors.log"), ex.ToString());
        }
    }

    private void Upload()
    {
        try
        {
            Compile();
            _cli.Upload(projectPath, "COM5", line => AddLog(line)); // COM порт можно вынести в настройки
            AddLog("Загрузка завершена");
        }
        catch (Exception ex)
        {
            AddLog("[Ошибка] " + ex.Message);
        }
    }
    
    public RobotAction? SelectedAction
    {
        get => _selectedAction;
        set
        {
            _selectedAction = value;
            OnPropertyChanged(nameof(SelectedAction));
            OnPropertyChanged(nameof(SelectedParameters));
            UpdatePreview();
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
            UpdatePreview();
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
            UpdatePreview();
        }
    }
    public string PreviewCode
    {
        get => _previewCode;
        set { _previewCode = value; OnPropertyChanged(nameof(PreviewCode)); }
    }

    public void UpdatePreview()
    {
        PreviewCode = _generator.GenerateCode(Actions);
    }

    private void AddLoop()
    {
        var loop = new LoopAction { RepeatCount = 2 };
        Actions.Add(loop);
        AddLog("Добавлен цикл x2");
        UpdatePreview();
    }

    private void AddConditional()
    {
        var cond = new ConditionalAction { Condition = "true" };
        Actions.Add(cond);
        AddLog("Добавлено условие");
        UpdatePreview();
    }
    private void RemoveFromParent(RobotAction action)
    {
        if (action.Parent != null)
            action.Parent.Children.Remove(action);
        else
            Actions.Remove(action);
    }
    private RobotAction CloneAction(RobotAction action)
    {
        if (action is LoopAction loop)
        {
            var copy = new LoopAction { RepeatCount = loop.RepeatCount };
            foreach (var child in loop.Children)
                copy.Children.Add(CloneAction(child));
            return copy;
        }
        else if (action is ConditionalAction cond)
        {
            var copy = new ConditionalAction { Condition = cond.Condition };
            foreach (var child in cond.Children)
                copy.Children.Add(CloneAction(child));
            return copy;
        }
        else if (action is CustomAction custom)
        {
            var copy = new CustomAction
            {
                TemplateName = custom.TemplateName,
                TemplateCode = custom.TemplateCode
            };

            foreach (var p in custom.Parameters)
                copy.Parameters.Add(new ParameterItem
                {
                    Name = p.Name,
                    Value = p.Value
                });

            return copy;
        }
        else if (action is MoveAction move)
        {
            var copy = new MoveAction();
            foreach (var p in move.Parameters)
                //  copy.Parameters.Add(new ParameterItem
                //  {
                //      Name = p.Name,
                //      Value = p.Value
                //   });
                return copy;
        }
        else if (action is WaitAction wait)
        {
            var copy = new WaitAction();
            foreach (var p in wait.Parameters)
                //  copy.Parameters.Add(new ParameterItem {   Name = p.Name,Value = p.Value });
                return copy;
        }

        return null;
    }
    private void DeleteSelected()
    {
        if (SelectedAction != null)
        {
            RemoveFromParent(SelectedAction); // удаляем из дерева или корневого списка
            SelectedAction = null;
            UpdatePreview();
        }
    }

    private void CopySelected()
    {
        if (SelectedAction != null)
            _clipboard = CloneAction(SelectedAction); // глубокое копирование
    }

    private void PasteSelected()
    {
        if (_clipboard == null) return;

        if (SelectedAction is ContainerAction container)
        {
            var copy = CloneAction(_clipboard);
            container.Children.Add(copy);
            copy.Parent = container;
        }
        else if (SelectedAction != null)
        {
            var copy = CloneAction(_clipboard);
            if (SelectedAction.Parent != null)
            {
                int index = SelectedAction.Parent.Children.IndexOf(SelectedAction);
                SelectedAction.Parent.Children.Insert(index + 1, copy);
                copy.Parent = SelectedAction.Parent;
            }
            else
            {
                Actions.Add(copy);
                copy.Parent = null;
            }
        }
        else
        {
            var copy = CloneAction(_clipboard);
            Actions.Add(copy);
            copy.Parent = null;
        }

        UpdatePreview();
    }
    
}

