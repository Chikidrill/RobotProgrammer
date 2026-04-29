using Microsoft.Win32;
using Model;
using Model.ArduinoServices;
using Model.RobotActions;
using Model.Services;
using System;
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
    private int _activeTabIndex = 3;

    public int ActiveTabIndex
    {
        get => _activeTabIndex;
        set
        {
            _activeTabIndex = value;

            ActiveTab = value switch
            {
                1 => "Setup",
                3 => "Autonomous",
                4 => TeleopTabIndex == 1 ? "TeleopButtons" : "TeleopAlways",
                _ => ActiveTab
            };

            SelectedAction = null;

            OnPropertyChanged(nameof(ActiveTabIndex));
            OnPropertyChanged(nameof(Actions));
            OnPropertyChanged(nameof(ActiveTab));
        }
    }
    public ObservableCollection<RobotAction> Actions
    {
        get
        {
            return ActiveTab switch
            {
                "Setup" => Program.Setup,
                "Autonomous" => Program.Autonomous,
                "TeleopAlways" => Program.Teleop.AlwaysRunning,
                "Teleop" => Program.Teleop.ButtonRules,
                _ => Program.Autonomous
            };
        }
    }
    public string[] Ps4Buttons { get; } =
{
    "L1",
    "L2",
    "L3",
    "R1",
    "R2",
    "R3",
    "UP",
    "DOWN",
    "RIGHT",
    "LEFT",
    "TRIANGLE",
    "CROSS",
    "CIRCLE",
    "SQUARE",
    "SHARE",
    "OPTIONS",
    "POWER",
    "TOUCH"
};

    public Array TriggerModes { get; } =
        Enum.GetValues(typeof(TriggerMode));
    public ObservableCollection<ProgramVariable> Variables { get; } = new();
    public event PropertyChangedEventHandler PropertyChanged;
    private readonly IDialogService _dialogService;
    public ObservableCollection<ActionParameter> SelectedParameters =>
    SelectedAction?.GetParameters() ?? new();
    private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    public RobotProgram Program { get; } = new RobotProgram();
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
    public ICommand AddVariableCommand { get; }
    public ICommand DeleteVariableCommand { get; }
    public ICommand AddButtonRuleCommand { get; }
    public ICommand DeleteButtonRuleCommand { get; }
    public ICommand AddFunctionCommand { get; }
    public ICommand DeleteFunctionCommand { get; }
    public ICommand AddFunctionParameterCommand { get; }
    public ICommand DeleteFunctionParameterCommand { get; }
    public ICommand SaveFunctionToLibraryCommand { get; }
    public ICommand ImportFunctionFromLibraryCommand { get; }
    public ICommand AddFunctionCallToSetupCommand { get; }
    public ICommand AddFunctionCallToAutonomousCommand { get; }
    public ICommand AddFunctionCallToTeleopAlwaysCommand { get; }
    public ICommand AddFunctionCallToSelectedButtonRuleCommand { get; }
    public ICommand AddLibraryFunctionCallCommand { get; }
    public Array VariableTypes { get; } =
    Enum.GetValues(typeof(ProgramVariableType));

    private readonly ArduinoCodeGenerator _generator = new();
    private readonly ArduinoCliService _cli = new();
    private string _activeTab = "Autonomous"; // по умолчанию
    private ProgramVariable? _selectedVariable;
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
        //Actions = new ObservableCollection<RobotAction>();
        DeleteSelectedCommand = new RelayCommand(DeleteSelected);
        CopySelectedCommand = new RelayCommand(CopySelected);
        PasteSelectedCommand = new RelayCommand(PasteSelected);
        AddVariableCommand = new RelayCommand(AddVariable);
        DeleteVariableCommand = new RelayCommand(DeleteVariable);
        AddButtonRuleCommand = new RelayCommand(AddButtonRule);
        DeleteButtonRuleCommand = new RelayCommand(DeleteButtonRule);
        AddFunctionCommand = new RelayCommand(AddFunction);
        DeleteFunctionCommand = new RelayCommand(DeleteFunction);
        AddFunctionParameterCommand = new RelayCommand(AddFunctionParameter);
        DeleteFunctionParameterCommand = new RelayCommand(DeleteFunctionParameter);
        SaveFunctionToLibraryCommand = new RelayCommand(SaveFunctionToLibrary);
        ImportFunctionFromLibraryCommand = new RelayCommand(ImportFunctionFromLibrary);
        AddFunctionCallToSetupCommand = new RelayCommand(AddFunctionCallToSetup);
        AddFunctionCallToAutonomousCommand = new RelayCommand(AddFunctionCallToAutonomous);
        AddFunctionCallToTeleopAlwaysCommand = new RelayCommand(AddFunctionCallToTeleopAlways);
        AddFunctionCallToSelectedButtonRuleCommand = new RelayCommand(AddFunctionCallToSelectedButtonRule);
        AddLibraryFunctionCallCommand = new RelayCommand(AddLibraryFunctionCall);
        LoadFunctionLibrary();
    }
    public string ActiveTab
    {
        get => _activeTab;
        set { _activeTab = value; OnPropertyChanged(nameof(ActiveTab)); OnPropertyChanged(nameof(Actions)); }
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
        AddRobotAction(new MoveAction());
        AddLog("Добавлено движение");
        UpdatePreview();
    }

    private void AddWait()
    {
        AddRobotAction(new WaitAction());
        AddLog("Добавлена пауза");
        UpdatePreview();
    }

    private void Compile()
    {
        try
        {
            Directory.CreateDirectory(projectPath);
            string code = _generator.GenerateCode(
                Program.Setup,
                Program.Autonomous,
                Program.Teleop,
                Variables,
                Program.Functions);
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
        PreviewCode = _generator.GenerateCode(
            Program.Setup,
            Program.Autonomous,
            Program.Teleop,
            Variables,
            Program.Functions);
    }

    private void AddLoop()
    {
        var loop = new LoopAction { RepeatCount = 2 };
        AddRobotAction(new LoopAction { RepeatCount = 2 });
        AddLog("Добавлен цикл x2");
        UpdatePreview();
    }

    private void AddConditional()
    {
        AddRobotAction(new ConditionalAction { Condition = "true" });
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
            var copy = new ConditionalAction
            {
                Condition = cond.Condition
            };

            copy.Children.Clear();

            foreach (var child in cond.Children)
            {
                var childCopy = CloneAction(child);
                childCopy.Parent = copy;
                copy.Children.Add(childCopy);
            }

            return copy;
        }
        if (action is BranchAction branch)
        {
            var copy = new BranchAction(branch.BranchName);

            foreach (var child in branch.Children)
            {
                var childCopy = CloneAction(child);
                childCopy.Parent = copy;
                copy.Children.Add(childCopy);
            }

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
    public ProgramVariable? SelectedVariable
    {
        get => _selectedVariable;
        set
        {
            _selectedVariable = value;
            OnPropertyChanged(nameof(SelectedVariable));
        }
    }
    private void AddVariable()
    {
        Variables.Add(new ProgramVariable
        {
            Type = ProgramVariableType.Int,
            Name = "NEW_VAR",
            DefaultValue = "0",
            IsConstant = true
        });

        AddLog("Добавлена переменная");
        UpdatePreview();
    }

    private void DeleteVariable()
    {
        if (SelectedVariable == null)
            return;

        Variables.Remove(SelectedVariable);
        SelectedVariable = null;

        AddLog("Переменная удалена");
        UpdatePreview();
    }
    private TeleopButtonRule? _selectedButtonRule;

    public TeleopButtonRule? SelectedButtonRule
    {
        get => _selectedButtonRule;
        set
        {
            _selectedButtonRule = value;
            OnPropertyChanged(nameof(SelectedButtonRule));
        }
    }
    private void AddButtonRule()
    {
        var rule = new TeleopButtonRule
        {
            Button = "TRIANGLE",
            TriggerMode = TriggerMode.WhilePressed
        };

        Program.Teleop.ButtonRules.Add(rule);

        AddLog("[Teleop] Добавлено правило кнопки");
        UpdatePreview();
    }

    private void DeleteButtonRule()
    {
        if (SelectedButtonRule == null)
            return;

        Program.Teleop.ButtonRules.Remove(SelectedButtonRule);
        SelectedButtonRule = null;

        AddLog("[Teleop] Правило кнопки удалено");
        UpdatePreview();
    }
    private int _teleopTabIndex;

    public int TeleopTabIndex
    {
        get => _teleopTabIndex;
        set
        {
            _teleopTabIndex = value;

            if (ActiveTabIndex == 3)
            {
                ActiveTab = value switch
                {
                    0 => "TeleopAlways",
                    1 => "TeleopButtons",
                    _ => "TeleopAlways"
                };

                SelectedAction = null;

                OnPropertyChanged(nameof(Actions));
                OnPropertyChanged(nameof(ActiveTab));
            }

            OnPropertyChanged(nameof(TeleopTabIndex));
        }
    }
    private void AddRobotAction(RobotAction action)
    {
        // Если мы на вкладке Teleop -> Buttons,
        // действие надо добавлять внутрь выбранного правила кнопки
        if (ActiveTab == "TeleopButtons")
        {
            if (SelectedButtonRule == null)
            {
                AddLog("[Teleop Buttons] Сначала выбери правило кнопки.");
                return;
            }

            SelectedButtonRule.Children.Add(action);
            action.Parent = SelectedButtonRule;

            AddLog($"[Teleop Buttons] Добавлено действие в кнопку {SelectedButtonRule.Button}: {action.DisplayType}");
            UpdatePreview();
            return;
        }

        // Если выбран контейнер: цикл, условие, IF/ELSE, правило и т.п.
        if (SelectedAction is ContainerAction container)
        {
            container.Children.Add(action);
            action.Parent = container;

            AddLog($"[{ActiveTab}] Добавлено действие внутрь {container.DisplayType}: {action.DisplayType}");
            UpdatePreview();
            return;
        }

        // Обычное добавление в текущий раздел
        Actions.Add(action);
        action.Parent = null;

        AddLog($"[{ActiveTab}] Добавлено действие: {action.DisplayType}");
        UpdatePreview();
    }
    public string[] CppTypes { get; } =
{
    "void",
    "bool",
    "int",
    "long",
    "float",
    "double",
    "String"
};

    public ObservableCollection<ProgramFunction> FunctionLibrary { get; } = new();

    private ProgramFunction? _selectedFunction;
    public ProgramFunction? SelectedFunction
    {
        get => _selectedFunction;
        set
        {
            _selectedFunction = value;
            OnPropertyChanged(nameof(SelectedFunction));
        }
    }

    private ProgramFunction? _selectedLibraryFunction;
    public ProgramFunction? SelectedLibraryFunction
    {
        get => _selectedLibraryFunction;
        set
        {
            _selectedLibraryFunction = value;
            OnPropertyChanged(nameof(SelectedLibraryFunction));
        }
    }

    private FunctionParameter? _selectedFunctionParameter;
    public FunctionParameter? SelectedFunctionParameter
    {
        get => _selectedFunctionParameter;
        set
        {
            _selectedFunctionParameter = value;
            OnPropertyChanged(nameof(SelectedFunctionParameter));
        }
    }
    private void LoadFunctionLibrary()
    {
        FunctionLibrary.Clear();

        foreach (var function in FunctionLibraryService.LoadAll())
            FunctionLibrary.Add(function);

        AddLog($"Загружено функций из библиотеки: {FunctionLibrary.Count}");
    }

    private void AddFunction()
    {
        var function = new ProgramFunction
        {
            Name = "driveByDegrees",
            ReturnType = "void",
            BodyCode =
    @"  prizm.resetEncoders();
  expansion.resetEncoders(1);

  prizm.setMotorDegree(2, 225, degLR);
  prizm.setMotorDegree(1, 225, degRR);

  expansion.setMotorDegree(1, 1, 225, degLF);
  expansion.setMotorDegree(1, 2, 225, degRF);"
        };

        function.Parameters.Add(new FunctionParameter { Type = "int", Name = "degLR" });
        function.Parameters.Add(new FunctionParameter { Type = "int", Name = "degRR" });
        function.Parameters.Add(new FunctionParameter { Type = "int", Name = "degLF" });
        function.Parameters.Add(new FunctionParameter { Type = "int", Name = "degRF" });

        Program.Functions.Add(function);
        SelectedFunction = function;

        AddLog("Добавлена функция");
        UpdatePreview();
    }

    private void DeleteFunction()
    {
        if (SelectedFunction == null)
            return;

        Program.Functions.Remove(SelectedFunction);
        SelectedFunction = null;

        AddLog("Функция удалена");
        UpdatePreview();
    }

    private void AddFunctionParameter()
    {
        if (SelectedFunction == null)
            return;

        SelectedFunction.Parameters.Add(new FunctionParameter
        {
            Type = "int",
            Name = "value"
        });

        UpdatePreview();
    }

    private void DeleteFunctionParameter()
    {
        if (SelectedFunction == null || SelectedFunctionParameter == null)
            return;

        SelectedFunction.Parameters.Remove(SelectedFunctionParameter);
        SelectedFunctionParameter = null;

        UpdatePreview();
    }

    private void SaveFunctionToLibrary()
    {
        if (SelectedFunction == null)
        {
            AddLog("Сначала выбери функцию.");
            return;
        }

        FunctionLibraryService.Save(SelectedFunction);

        LoadFunctionLibrary();

        AddLog($"Функция сохранена в библиотеку: {SelectedFunction.Name}");
    }

    private void ImportFunctionFromLibrary()
    {
        if (SelectedLibraryFunction == null)
        {
            AddLog("Сначала выбери функцию из библиотеки.");
            return;
        }

        var copy = CloneProgramFunction(SelectedLibraryFunction);

        Program.Functions.Add(copy);
        SelectedFunction = copy;

        AddLog($"Функция добавлена в проект: {copy.Name}");
        UpdatePreview();
    }

    private string GetDefaultArgumentValue(string type)
    {
        return type switch
        {
            "bool" => "false",
            "float" => "0.0",
            "double" => "0.0",
            "String" => "\"\"",
            _ => "0"
        };
    }

    private ProgramFunction CloneProgramFunction(ProgramFunction source)
    {
        var copy = new ProgramFunction
        {
            Name = source.Name,
            ReturnType = source.ReturnType,
            BodyCode = source.BodyCode
        };

        foreach (var p in source.Parameters)
        {
            copy.Parameters.Add(new FunctionParameter
            {
                Type = p.Type,
                Name = p.Name
            });
        }

        return copy;
    }
    private void AddLibraryFunctionCall()
    {
        if (SelectedLibraryFunction == null)
        {
            AddLog("Сначала выбери функцию из библиотеки.");
            return;
        }

        var projectFunction = EnsureFunctionInProject(SelectedLibraryFunction);

        SelectedFunction = projectFunction;

        AddFunctionCallFor(projectFunction);
    }
    private ProgramFunction EnsureFunctionInProject(ProgramFunction libraryFunction)
    {
        var existing = Program.Functions
            .FirstOrDefault(f => f.SafeName == libraryFunction.SafeName);

        if (existing != null)
            return existing;

        var copy = CloneProgramFunction(libraryFunction);

        Program.Functions.Add(copy);

        AddLog($"Функция добавлена в проект: {copy.Name}");

        return copy;
    }
    private void AddFunctionCallFor(ProgramFunction function)
    {
        var call = new FunctionCallAction
        {
            FunctionName = function.SafeName
        };

        foreach (var p in function.Parameters)
        {
            call.Arguments.Add(new ParameterItem
            {
                Name = p.Name,
                Value = GetDefaultArgumentValue(p.Type)
            });
        }

        AddRobotAction(call);

        AddLog($"Добавлен вызов функции: {function.Name}");
        UpdatePreview();
    }
    private ProgramFunction? GetSelectedFunctionForCall()
    {
        if (SelectedFunction != null)
            return SelectedFunction;

        if (SelectedLibraryFunction != null)
            return EnsureFunctionInProject(SelectedLibraryFunction);

        AddLog("Сначала выбери функцию проекта или функцию из библиотеки.");
        return null;
    }
    private FunctionCallAction CreateFunctionCall(ProgramFunction function)
    {
        var call = new FunctionCallAction
        {
            FunctionName = function.SafeName
        };

        foreach (var p in function.Parameters)
        {
            call.Arguments.Add(new ParameterItem
            {
                Name = p.Name,
                Value = GetDefaultArgumentValue(p.Type)
            });
        }

        return call;
    }
    private void AddFunctionCallToSetup()
    {
        var function = GetSelectedFunctionForCall();
        if (function == null)
            return;

        var call = CreateFunctionCall(function);

        Program.Setup.Add(call);
        call.Parent = null;

        AddLog($"[Setup] Добавлен вызов функции: {function.Name}");
        UpdatePreview();
    }

    private void AddFunctionCallToAutonomous()
    {
        var function = GetSelectedFunctionForCall();
        if (function == null)
            return;

        var call = CreateFunctionCall(function);

        Program.Autonomous.Add(call);
        call.Parent = null;

        AddLog($"[Autonomous] Добавлен вызов функции: {function.Name}");
        UpdatePreview();
    }

    private void AddFunctionCallToTeleopAlways()
    {
        var function = GetSelectedFunctionForCall();
        if (function == null)
            return;

        var call = CreateFunctionCall(function);

        Program.Teleop.AlwaysRunning.Add(call);
        call.Parent = null;

        AddLog($"[Teleop Always] Добавлен вызов функции: {function.Name}");
        UpdatePreview();
    }

    private void AddFunctionCallToSelectedButtonRule()
    {
        var function = GetSelectedFunctionForCall();
        if (function == null)
            return;

        if (SelectedButtonRule == null)
        {
            AddLog("[Teleop Buttons] Сначала выбери правило кнопки.");
            return;
        }

        var call = CreateFunctionCall(function);

        SelectedButtonRule.Children.Add(call);
        call.Parent = SelectedButtonRule;

        AddLog($"[Teleop Buttons] В кнопку {SelectedButtonRule.Button} добавлен вызов функции: {function.Name}");
        UpdatePreview();
    }
}

