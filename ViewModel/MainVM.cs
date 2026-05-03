using Model;
using Model.ArduinoServices;
using Model.RobotActions;
using Model.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Input;
using ViewModel;
using System.IO.Ports;
using Velopack;
using Velopack.Sources;

namespace RobotProgrammer.ViewModel;

public class MainVM : INotifyPropertyChanged
{
    #region Fields

    private const string DefaultProjectPath = "robot";

    private readonly IFileDialogService _fileDialog;
    private readonly IWindowService _windowService;
    private readonly ArduinoCodeGenerator _generator = new();
    private readonly ArduinoCliService _cli = new();

    private RobotProgram _program = new();
    private RobotAction? _selectedAction;
    private RobotAction? _clipboard;

    private ProgramVariable? _selectedVariable;
    private TeleopButtonRule? _selectedButtonRule;

    private ProgramFunction? _selectedFunction;
    private ProgramFunction? _selectedLibraryFunction;
    private FunctionParameter? _selectedFunctionParameter;

    private string _activeTab = "Autonomous";
    private int _activeTabIndex = 5;
    private int _teleopTabIndex;
    private string _previewCode = string.Empty;
    private CustomAction? _selectedTemplate;

    private AutonomousRoutine? _selectedAutonomousRoutine;
    private string _newAutonomousRoutineName = "New autonomous";

    private ProgramInclude? _selectedInclude;
    private string? _selectedComPort;
    #endregion

    #region Constructor

    public MainVM(
        IFileDialogService fileDialog,
        IWindowService windowService,
        IDialogService dialogService)
    {
        _fileDialog = fileDialog;
        _windowService = windowService;
        _ = dialogService;

        AddMoveCommand = new RelayCommand(AddMove);
        AddWaitCommand = new RelayCommand(AddWait);
        AddLoopCommand = new RelayCommand(AddLoop);
        AddConditionalCommand = new RelayCommand(AddConditional);

        SaveAsProjectCommand = new RelayCommand(SaveAsProject);
        OpenProjectCommand = new RelayCommand(OpenProject);
        CompileCommand = new RelayCommand(Compile);
        UploadCommand = new RelayCommand(Upload);

        NewTemplateCommand = new RelayCommand(OpenNewTemplateWindow);
        LoadTemplateCommand = new RelayCommand(OpenTemplatePicker);
        EditTemplateCommand = new RelayCommand(EditTemplate);

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
        RefreshTemplateLibraryCommand = new RelayCommand(() => LoadTemplateLibrary(logResult: true));

        AddTemplateToSetupCommand = new RelayCommand(AddTemplateToSetup);
        AddTemplateToAutonomousCommand = new RelayCommand(AddTemplateToAutonomous);
        AddTemplateToTeleopAlwaysCommand = new RelayCommand(AddTemplateToTeleopAlways);
        AddTemplateToSelectedButtonRuleCommand = new RelayCommand(AddTemplateToSelectedButtonRule);

        AddFunctionCallToCurrentContextCommand = new RelayCommand(AddFunctionCallToCurrentContext);
        AddTemplateToCurrentContextCommand = new RelayCommand(AddTemplateToCurrentContext);
        EditSelectedTemplateCommand = new RelayCommand(EditSelectedTemplate);

        RefreshAutonomousLibraryCommand = new RelayCommand(() => LoadAutonomousLibrary(logResult: true));
        SaveCurrentAutonomousCommand = new RelayCommand(SaveCurrentAutonomous);
        LoadSelectedAutonomousCommand = new RelayCommand(LoadSelectedAutonomous);
        NewAutonomousCommand = new RelayCommand(NewAutonomous);

        AddIncludeCommand = new RelayCommand(AddInclude);
        DeleteIncludeCommand = new RelayCommand(DeleteInclude);
        RefreshComPortsCommand = new RelayCommand(RefreshComPorts);
        CheckUpdatesCommand = new RelayCommand(CheckUpdates);

        RefreshComPorts();
        LoadAutonomousLibrary(logResult: true);
        LoadTemplateLibrary(logResult: true);
        LoadFunctionLibrary(logResult: true);
        UpdatePreview();
    }

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion

    #region Public data

    public RobotProgram Program
    {
        get => _program;
        private set
        {
            _program = value;
            OnPropertyChanged(nameof(Program));
            OnPropertyChanged(nameof(Includes));
            OnPropertyChanged(nameof(Variables));
            OnPropertyChanged(nameof(Actions));
        }
    }

    public ObservableCollection<ProgramVariable> Variables => Program.Variables;

    public ObservableCollection<RobotAction> Actions
    {
        get
        {
            return ActiveTab switch
            {
                "Setup" => Program.Setup,
                "Autonomous" => Program.Autonomous,
                "TeleopAlways" => Program.Teleop.AlwaysRunning,
                _ => Program.Autonomous
            };
        }
    }

    public ObservableCollection<string> Log { get; } = new();

    public string LogString => string.Join(Environment.NewLine, Log);

    public ObservableCollection<ProgramFunction> FunctionLibrary { get; } = new();

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

    public ObservableCollection<ActionParameter> SelectedParameters =>
        SelectedAction?.GetParameters() ?? new ObservableCollection<ActionParameter>();
    public ObservableCollection<CustomAction> TemplateLibrary { get; } = new();
    public ObservableCollection<AutonomousRoutine> AutonomousLibrary { get; } = new();
    public ObservableCollection<ProgramInclude> Includes => Program.Includes;

    public Array IncludeKinds { get; } =
        Enum.GetValues(typeof(ProgramIncludeKind));

    public ObservableCollection<string> AvailableComPorts { get; } = new();
    #endregion

    #region Selected items

    public RobotAction? SelectedAction
    {
        get => _selectedAction;
        set
        {
            _selectedAction = value;
            OnPropertyChanged(nameof(SelectedAction));
            OnPropertyChanged(nameof(SelectedParameters));
        }
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

    public TeleopButtonRule? SelectedButtonRule
    {
        get => _selectedButtonRule;
        set
        {
            _selectedButtonRule = value;

            if (value != null)
                SelectedAction = null;

            OnPropertyChanged(nameof(SelectedButtonRule));
        }
    }

    public ProgramFunction? SelectedFunction
    {
        get => _selectedFunction;
        set
        {
            _selectedFunction = value;

            if (value != null)
            {
                _selectedLibraryFunction = null;
                OnPropertyChanged(nameof(SelectedLibraryFunction));
            }

            OnPropertyChanged(nameof(SelectedFunction));
        }
    }

    public ProgramFunction? SelectedLibraryFunction
    {
        get => _selectedLibraryFunction;
        set
        {
            _selectedLibraryFunction = value;

            if (value != null)
            {
                _selectedFunction = null;
                OnPropertyChanged(nameof(SelectedFunction));
            }

            OnPropertyChanged(nameof(SelectedLibraryFunction));
        }
    }

    public FunctionParameter? SelectedFunctionParameter
    {
        get => _selectedFunctionParameter;
        set
        {
            _selectedFunctionParameter = value;
            OnPropertyChanged(nameof(SelectedFunctionParameter));
        }
    }


    public CustomAction? SelectedTemplate
    {
        get => _selectedTemplate;
        set
        {
            _selectedTemplate = value;
            OnPropertyChanged(nameof(SelectedTemplate));
        }
    }

    public ProgramInclude? SelectedInclude
    {
        get => _selectedInclude;
        set
        {
            _selectedInclude = value;
            OnPropertyChanged(nameof(SelectedInclude));
        }
    }

    public string? SelectedComPort
    {
        get => _selectedComPort;
        set
        {
            _selectedComPort = value;
            OnPropertyChanged(nameof(SelectedComPort));
        }
    }

    #endregion

    #region Navigation

    public string ActiveTab
    {
        get => _activeTab;
        set
        {
            if (_activeTab == value)
                return;

            _activeTab = value;
            OnPropertyChanged(nameof(ActiveTab));
            OnPropertyChanged(nameof(Actions));
        }
    }

    public int ActiveTabIndex
    {
        get => _activeTabIndex;
        set
        {
            if (_activeTabIndex == value)
                return;

            _activeTabIndex = value;

            ActiveTab = value switch
            {
                2 => "Setup",
                4 => "Autonomous",
                5 => TeleopTabIndex == 1 ? "TeleopButtons" : "TeleopAlways",
                _ => ActiveTab
            };

            SelectedAction = null;

            OnPropertyChanged(nameof(ActiveTabIndex));
            OnPropertyChanged(nameof(Actions));
            OnPropertyChanged(nameof(ActiveTab));
        }
    }

    public int TeleopTabIndex
    {
        get => _teleopTabIndex;
        set
        {
            if (_teleopTabIndex == value)
                return;

            _teleopTabIndex = value;

            if (ActiveTabIndex == 5)
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

    #endregion

    #region Preview

    public string PreviewCode
    {
        get => _previewCode;
        set
        {
            _previewCode = value;
            OnPropertyChanged(nameof(PreviewCode));
        }
    }

    public void UpdatePreview()
    {
        try
        {
            PreviewCode = GenerateSketchCode();
        }
        catch (Exception ex)
        {
            PreviewCode = $"// Ошибка генерации preview:{Environment.NewLine}// {ex.Message}";
        }
    }

    private string GenerateSketchCode()
    {
        return _generator.GenerateCode(Program);
    }

    #endregion

    #region Commands

    public ICommand AddMoveCommand { get; }
    public ICommand AddWaitCommand { get; }
    public ICommand AddLoopCommand { get; }
    public ICommand AddConditionalCommand { get; }

    public ICommand SaveAsProjectCommand { get; }
    public ICommand OpenProjectCommand { get; }
    public ICommand CompileCommand { get; }
    public ICommand UploadCommand { get; }

    public ICommand NewTemplateCommand { get; }
    public ICommand LoadTemplateCommand { get; }
    public ICommand EditTemplateCommand { get; }

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
    public ICommand RefreshTemplateLibraryCommand { get; }

    public ICommand AddTemplateToSetupCommand { get; }
    public ICommand AddTemplateToAutonomousCommand { get; }
    public ICommand AddTemplateToTeleopAlwaysCommand { get; }
    public ICommand AddTemplateToSelectedButtonRuleCommand { get; }
    public ICommand AddFunctionCallToCurrentContextCommand { get; }
    public ICommand AddTemplateToCurrentContextCommand { get; }
    public ICommand EditSelectedTemplateCommand { get; }

    public ICommand RefreshAutonomousLibraryCommand { get; }
    public ICommand SaveCurrentAutonomousCommand { get; }
    public ICommand LoadSelectedAutonomousCommand { get; }
    public ICommand NewAutonomousCommand { get; }

    public ICommand AddIncludeCommand { get; }
    public ICommand DeleteIncludeCommand { get; }
    public ICommand RefreshComPortsCommand { get; }
    public ICommand CheckUpdatesCommand { get; }

    #endregion

    #region Project file

    private void SaveAsProject()
    {
        var path = _fileDialog.SaveFile(
            "Robot project (*.rproj)|*.rproj",
            ".rproj");

        if (path == null)
            return;

        try
        {
            var json = JsonSerializer.Serialize(
                Program,
                GetProjectJsonOptions());

            File.WriteAllText(path, json);

            AddLog($"Проект сохранён: {path}");
        }
        catch (Exception ex)
        {
            AddLog("[Ошибка сохранения проекта] " + ex.Message);
        }
    }

    private void OpenProject()
    {
        var path = _fileDialog.OpenFile("Robot project (*.rproj)|*.rproj");

        if (path == null)
            return;

        try
        {
            var json = File.ReadAllText(path);
            Program = LoadRobotProgramFromJson(json);

            RestoreAllParents();

            SelectedAction = null;
            SelectedVariable = null;
            SelectedFunction = null;
            SelectedLibraryFunction = null;
            SelectedButtonRule = null;
            SelectedFunctionParameter = null;

            AddLog($"Проект загружен: {path}");
            UpdatePreview();
        }
        catch (Exception ex)
        {
            AddLog("[Ошибка загрузки проекта] " + ex.Message);
        }
    }

    private static JsonSerializerOptions GetProjectJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        options.Converters.Add(new RobotActionConverter());

        return options;
    }

    private static RobotProgram LoadRobotProgramFromJson(string json)
    {
        var options = GetProjectJsonOptions();

        try
        {
            var program = JsonSerializer.Deserialize<RobotProgram>(json, options);

            if (program != null)
                return program;
        }
        catch (JsonException)
        {
            // Пробуем открыть старый формат проекта ниже.
        }

        var legacyActions = JsonSerializer.Deserialize<List<RobotAction>>(json, options)
                            ?? new List<RobotAction>();

        var legacyProgram = new RobotProgram();

        foreach (var action in legacyActions)
            legacyProgram.Autonomous.Add(action);

        return legacyProgram;
    }

    #endregion

    #region Compile / upload

    private void Compile()
    {
        CompileSketch();
    }

    private bool CompileSketch()
    {
        try
        {
            Directory.CreateDirectory(DefaultProjectPath);

            var code = GenerateSketchCode();
            var sketchPath = Path.Combine(DefaultProjectPath, "robot.ino");

            File.WriteAllText(sketchPath, code);

            AddLog("Код сгенерирован");

            try
            {
                _cli.Compile(DefaultProjectPath, AddLog);
                AddLog("Компиляция завершена");
                return true;
            }
            catch (Exception ex)
            {
                WriteCompileError(ex);
                AddLog("[Ошибка компиляции] " + ex.Message);
                return false;
            }
        }
        catch (Exception ex)
        {
            WriteCompileError(ex);
            AddLog("[Ошибка] " + ex.Message);
            return false;
        }
    }

    private void Upload()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(SelectedComPort))
            {
                AddLog("[Ошибка загрузки] Сначала выбери COM-порт.");
                RefreshComPorts();
                return;
            }

            if (!CompileSketch())
                return;

            _cli.Upload(DefaultProjectPath, SelectedComPort, AddLog);
            AddLog($"Загрузка завершена на порт {SelectedComPort}");
        }
        catch (Exception ex)
        {
            AddLog("[Ошибка загрузки] " + ex.Message);
        }
    }

    private static void WriteCompileError(Exception ex)
    {
        Directory.CreateDirectory(DefaultProjectPath);

        var logPath = Path.Combine(DefaultProjectPath, "compile_errors.log");
        File.WriteAllText(logPath, ex.ToString());
    }

    #endregion

    #region Actions

    private void AddMove()
    {
        AddRobotAction(new MoveAction());
    }

    private void AddWait()
    {
        AddRobotAction(new WaitAction());
    }

    private void AddLoop()
    {
        AddRobotAction(new LoopAction { RepeatCount = 2 });
    }

    private void AddConditional()
    {
        AddRobotAction(new ConditionalAction { Condition = "true" });
    }

    private void AddRobotAction(RobotAction action)
    {
        if (ActiveTab == "TeleopButtons")
        {
            AddRobotActionToTeleopButton(action);
            return;
        }

        if (SelectedAction is ContainerAction selectedContainer)
        {
            AddActionToContainer(selectedContainer, action);
            AddLog($"[{ActiveTab}] Добавлено внутрь {selectedContainer.DisplayType}: {action.DisplayType}");
            UpdatePreview();
            return;
        }

        Actions.Add(action);
        action.Parent = null;

        AddLog($"[{ActiveTab}] Добавлено действие: {action.DisplayType}");
        UpdatePreview();
    }

    private void AddRobotActionToTeleopButton(RobotAction action)
    {
        if (SelectedAction is ContainerAction selectedContainer)
        {
            AddActionToContainer(selectedContainer, action);
            AddLog($"[Teleop Buttons] Добавлено внутрь {selectedContainer.DisplayType}: {action.DisplayType}");
            UpdatePreview();
            return;
        }

        if (SelectedButtonRule == null)
        {
            AddLog("[Teleop Buttons] Сначала выбери правило кнопки.");
            return;
        }

        AddActionToContainer(SelectedButtonRule, action);

        AddLog($"[Teleop Buttons] Добавлено в кнопку {SelectedButtonRule.Button}: {action.DisplayType}");
        UpdatePreview();
    }

    private static void AddActionToContainer(ContainerAction container, RobotAction action)
    {
        container.Children.Add(action);
        action.Parent = container;
    }

    #endregion

    #region Copy / paste / delete

    private void DeleteSelected()
    {
        if (SelectedAction == null)
            return;

        RemoveFromParent(SelectedAction);
        SelectedAction = null;

        UpdatePreview();
    }

    private void CopySelected()
    {
        if (SelectedAction == null)
            return;

        try
        {
            _clipboard = CloneAction(SelectedAction);
            AddLog($"Скопировано: {SelectedAction.DisplayType}");
        }
        catch (Exception ex)
        {
            AddLog("[Ошибка копирования] " + ex.Message);
        }
    }

    private void PasteSelected()
    {
        if (_clipboard == null)
            return;

        var copy = CloneAction(_clipboard);

        if (SelectedAction is ContainerAction selectedContainer)
        {
            AddActionToContainer(selectedContainer, copy);
            AddLog($"Вставлено внутрь {selectedContainer.DisplayType}: {copy.DisplayType}");
            UpdatePreview();
            return;
        }

        if (SelectedAction != null)
        {
            InsertAfterSelectedAction(copy);
            AddLog($"Вставлено после {SelectedAction.DisplayType}: {copy.DisplayType}");
            UpdatePreview();
            return;
        }

        if (ActiveTab == "TeleopButtons" && SelectedButtonRule != null)
        {
            AddActionToContainer(SelectedButtonRule, copy);
            AddLog($"Вставлено в кнопку {SelectedButtonRule.Button}: {copy.DisplayType}");
            UpdatePreview();
            return;
        }

        Actions.Add(copy);
        copy.Parent = null;

        AddLog($"Вставлено: {copy.DisplayType}");
        UpdatePreview();
    }

    private void InsertAfterSelectedAction(RobotAction copy)
    {
        if (SelectedAction == null)
            return;

        if (SelectedAction.Parent != null)
        {
            var parent = SelectedAction.Parent;
            var index = parent.Children.IndexOf(SelectedAction);

            parent.Children.Insert(index + 1, copy);
            copy.Parent = parent;

            return;
        }

        var rootIndex = Actions.IndexOf(SelectedAction);

        if (rootIndex >= 0)
        {
            Actions.Insert(rootIndex + 1, copy);
            copy.Parent = null;
        }
        else
        {
            Actions.Add(copy);
            copy.Parent = null;
        }
    }

    private void RemoveFromParent(RobotAction action)
    {
        if (action.Parent != null)
        {
            action.Parent.Children.Remove(action);
            return;
        }

        Actions.Remove(action);
    }

    private RobotAction CloneAction(RobotAction action)
    {
        switch (action)
        {
            case LoopAction loop:
                {
                    var copy = new LoopAction
                    {
                        RepeatCount = loop.RepeatCount
                    };

                    CloneChildren(loop, copy);
                    return copy;
                }

            case ConditionalAction conditional:
                {
                    var copy = new ConditionalAction
                    {
                        Condition = conditional.Condition
                    };

                    copy.Children.Clear();
                    CloneChildren(conditional, copy);

                    return copy;
                }

            case BranchAction branch:
                {
                    var copy = new BranchAction(branch.BranchName);
                    CloneChildren(branch, copy);
                    return copy;
                }

            case TeleopButtonRule buttonRule:
                {
                    var copy = new TeleopButtonRule
                    {
                        Button = buttonRule.Button,
                        TriggerMode = buttonRule.TriggerMode
                    };

                    CloneChildren(buttonRule, copy);
                    return copy;
                }

            case CustomAction custom:
                {
                    return CloneCustomAction(custom);
                }

            case FunctionCallAction call:
                {
                    var copy = new FunctionCallAction
                    {
                        FunctionName = call.FunctionName
                    };

                    foreach (var argument in call.Arguments)
                    {
                        copy.Arguments.Add(new ParameterItem
                        {
                            Name = argument.Name,
                            Value = argument.Value
                        });
                    }

                    return copy;
                }

            case MoveAction move:
                {
                    var copy = new MoveAction();
                    copy.ApplyParameters(move.GetParameters());
                    return copy;
                }

            case WaitAction wait:
                {
                    var copy = new WaitAction();
                    copy.ApplyParameters(wait.GetParameters());
                    return copy;
                }

            default:
                throw new NotSupportedException($"Неизвестный тип действия: {action.GetType().Name}");
        }
    }

    private void CloneChildren(ContainerAction source, ContainerAction target)
    {
        foreach (var child in source.Children)
        {
            var childCopy = CloneAction(child);
            childCopy.Parent = target;
            target.Children.Add(childCopy);
        }
    }

    #endregion

    #region Variables

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

    #endregion

    #region Templates

    private void OpenNewTemplateWindow()
    {
        var templateVM = new NewTemplateVM();

        if (_windowService.ShowDialog(templateVM) != true)
            return;

        TemplateService.SaveTemplate(templateVM.Result);
        LoadTemplateLibrary();

        var actionCopy = CloneCustomAction(templateVM.Result);
        AddRobotAction(actionCopy);

        AddLog($"Создан и сохранён пользовательский блок: {templateVM.Result.TemplateName}");
    }

    private void OpenTemplatePicker()
    {
        LoadTemplateLibrary(logResult: true);

        if (TemplateLibrary.Count == 0)
        {
            AddLog("Пользовательских блоков нет.");
            return;
        }

        AddLog("Библиотека пользовательских блоков обновлена. Выбери блок во вкладке Templates.");
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

        vm.Parameters.Clear();

        foreach (var parameter in template.Parameters)
        {
            vm.Parameters.Add(new ParameterItem
            {
                Name = parameter.Name,
                Value = parameter.Value
            });
        }

        if (_windowService.ShowDialog(vm) != true)
            return;

        template.TemplateName = vm.TemplateName;
        template.TemplateCode = vm.TemplateCode;

        template.Parameters.Clear();

        foreach (var parameter in vm.Parameters)
        {
            template.Parameters.Add(new ParameterItem
            {
                Name = parameter.Name,
                Value = parameter.Value
            });
        }

        TemplateService.SaveTemplate(template);

        AddLog($"Шаблон '{template.TemplateName}' обновлён");
        UpdatePreview();
    }

    #endregion

    #region Teleop buttons

    private void AddButtonRule()
    {
        var rule = new TeleopButtonRule
        {
            Button = "TRIANGLE",
            TriggerMode = TriggerMode.WhilePressed
        };

        Program.Teleop.ButtonRules.Add(rule);
        SelectedButtonRule = rule;

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

    #endregion

    #region Functions

    private void LoadFunctionLibrary(bool logResult = false)
    {
        FunctionLibrary.Clear();

        foreach (var function in FunctionLibraryService.LoadAll())
            FunctionLibrary.Add(function);

        if (logResult)
            AddLog($"Загружено функций из библиотеки: {FunctionLibrary.Count}");
    }

    private void AddFunction()
    {
        var function = CreateDefaultFunction();

        Program.Functions.Add(function);
        SelectedFunction = function;

        AddLog("Добавлена функция");
        UpdatePreview();
    }

    private static ProgramFunction CreateDefaultFunction()
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

        return function;
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

    private ProgramFunction? GetSelectedFunctionForCall()
    {
        if (SelectedFunction != null)
            return SelectedFunction;

        if (SelectedLibraryFunction != null)
            return EnsureFunctionInProject(SelectedLibraryFunction);

        AddLog("Сначала выбери функцию проекта или функцию из библиотеки.");
        return null;
    }

    private ProgramFunction EnsureFunctionInProject(ProgramFunction libraryFunction)
    {
        var existing = Program.Functions
            .FirstOrDefault(function => function.SafeName == libraryFunction.SafeName);

        if (existing != null)
            return existing;

        var copy = CloneProgramFunction(libraryFunction);

        Program.Functions.Add(copy);

        AddLog($"Функция добавлена в проект: {copy.Name}");

        return copy;
    }

    private static ProgramFunction CloneProgramFunction(ProgramFunction source)
    {
        var copy = new ProgramFunction
        {
            Name = source.Name,
            ReturnType = source.ReturnType,
            BodyCode = source.BodyCode
        };

        foreach (var parameter in source.Parameters)
        {
            copy.Parameters.Add(new FunctionParameter
            {
                Type = parameter.Type,
                Name = parameter.Name
            });
        }

        return copy;
    }

    private FunctionCallAction CreateFunctionCall(ProgramFunction function)
    {
        var call = new FunctionCallAction
        {
            FunctionName = function.SafeName
        };

        foreach (var parameter in function.Parameters)
        {
            call.Arguments.Add(new ParameterItem
            {
                Name = parameter.Name,
                Value = GetDefaultArgumentValue(parameter.Type)
            });
        }

        return call;
    }

    private static string GetDefaultArgumentValue(string type)
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

        AddActionToContainer(SelectedButtonRule, call);

        AddLog($"[Teleop Buttons] В кнопку {SelectedButtonRule.Button} добавлен вызов функции: {function.Name}");
        UpdatePreview();
    }

    #endregion

    #region Parent restore

    private void RestoreAllParents()
    {
        RestoreParents(Program.Setup);
        RestoreParents(Program.Autonomous);
        RestoreParents(Program.Teleop.AlwaysRunning);

        foreach (var rule in Program.Teleop.ButtonRules.OfType<TeleopButtonRule>())
        {
            rule.Parent = null;
            RestoreParents(rule.Children, rule);
        }
    }

    private static void RestoreParents(IEnumerable<RobotAction> actions, ContainerAction? parent = null)
    {
        foreach (var action in actions)
        {
            action.Parent = parent;

            if (action is ContainerAction container)
                RestoreParents(container.Children, container);
        }
    }

    #endregion

    #region Logging

    private void AddLog(string message)
    {
        Log.Add(message);
        OnPropertyChanged(nameof(LogString));
    }

    #endregion
    #region Template library

    private void LoadTemplateLibrary(bool logResult = false)
    {
        TemplateLibrary.Clear();

        foreach (var template in TemplateService.LoadAllTemplates())
            TemplateLibrary.Add(CloneCustomAction(template));

        if (logResult)
            AddLog($"Загружено пользовательских блоков: {TemplateLibrary.Count}");
    }

    private CustomAction? GetSelectedTemplateClone()
    {
        if (SelectedTemplate == null)
        {
            AddLog("Сначала выбери пользовательский блок из библиотеки.");
            return null;
        }

        return CloneCustomAction(SelectedTemplate);
    }

    private static CustomAction CloneCustomAction(CustomAction source)
    {
        var copy = new CustomAction
        {
            TemplateName = source.TemplateName,
            TemplateCode = source.TemplateCode
        };

        foreach (var parameter in source.Parameters)
        {
            copy.Parameters.Add(new ParameterItem
            {
                Name = parameter.Name,
                Value = parameter.Value
            });
        }

        return copy;
    }

    private void AddTemplateToSetup()
    {
        var template = GetSelectedTemplateClone();

        if (template == null)
            return;

        Program.Setup.Add(template);
        template.Parent = null;

        AddLog($"[Setup] Добавлен пользовательский блок: {template.TemplateName}");
        UpdatePreview();
    }

    private void AddTemplateToAutonomous()
    {
        var template = GetSelectedTemplateClone();

        if (template == null)
            return;

        Program.Autonomous.Add(template);
        template.Parent = null;

        AddLog($"[Autonomous] Добавлен пользовательский блок: {template.TemplateName}");
        UpdatePreview();
    }

    private void AddTemplateToTeleopAlways()
    {
        var template = GetSelectedTemplateClone();

        if (template == null)
            return;

        Program.Teleop.AlwaysRunning.Add(template);
        template.Parent = null;

        AddLog($"[Teleop Always] Добавлен пользовательский блок: {template.TemplateName}");
        UpdatePreview();
    }

    private void AddTemplateToSelectedButtonRule()
    {
        var template = GetSelectedTemplateClone();

        if (template == null)
            return;

        if (SelectedButtonRule == null)
        {
            AddLog("[Teleop Buttons] Сначала выбери правило кнопки.");
            return;
        }

        AddActionToContainer(SelectedButtonRule, template);

        AddLog($"[Teleop Buttons] В кнопку {SelectedButtonRule.Button} добавлен пользовательский блок: {template.TemplateName}");
        UpdatePreview();
    }

    #endregion

    #region ContextMenu

    private void AddFunctionCallToCurrentContext()
    {
        var function = GetSelectedFunctionForCall();

        if (function == null)
            return;

        var call = CreateFunctionCall(function);

        AddRobotAction(call);

        AddLog($"Добавлен вызов функции в текущий контекст: {function.Name}");
        UpdatePreview();
    }

    private void AddTemplateToCurrentContext()
    {
        var template = GetSelectedTemplateClone();

        if (template == null)
            return;

        AddRobotAction(template);

        AddLog($"Добавлен пользовательский блок в текущий контекст: {template.TemplateName}");
        UpdatePreview();
    }

    private void EditSelectedTemplate()
    {
        if (SelectedTemplate == null)
        {
            AddLog("Сначала выбери пользовательский блок из библиотеки.");
            return;
        }

        var vm = new NewTemplateVM
        {
            TemplateName = SelectedTemplate.TemplateName,
            TemplateCode = SelectedTemplate.TemplateCode
        };

        vm.Parameters.Clear();

        foreach (var parameter in SelectedTemplate.Parameters)
        {
            vm.Parameters.Add(new ParameterItem
            {
                Name = parameter.Name,
                Value = parameter.Value
            });
        }

        if (_windowService.ShowDialog(vm) != true)
            return;

        var updated = new CustomAction
        {
            TemplateName = vm.TemplateName,
            TemplateCode = vm.TemplateCode
        };

        foreach (var parameter in vm.Parameters)
        {
            updated.Parameters.Add(new ParameterItem
            {
                Name = parameter.Name,
                Value = parameter.Value
            });
        }

        TemplateService.SaveTemplate(updated);
        LoadTemplateLibrary(logResult: false);

        AddLog($"Пользовательский блок обновлён: {updated.TemplateName}");
    }
    #endregion

    #region Autonomous Variability
    public AutonomousRoutine? SelectedAutonomousRoutine
    {
        get => _selectedAutonomousRoutine;
        set
        {
            _selectedAutonomousRoutine = value;
            OnPropertyChanged(nameof(SelectedAutonomousRoutine));
        }
    }

    public string NewAutonomousRoutineName
    {
        get => _newAutonomousRoutineName;
        set
        {
            _newAutonomousRoutineName = value;
            OnPropertyChanged(nameof(NewAutonomousRoutineName));
        }
    }
    #endregion

    #region Autonomous library

    private void LoadAutonomousLibrary(bool logResult = false)
    {
        AutonomousLibrary.Clear();

        foreach (var routine in AutonomousLibraryService.LoadAll())
        {
            RestoreParents(routine.Actions);
            AutonomousLibrary.Add(routine);
        }

        if (logResult)
            AddLog($"Загружено автономок из библиотеки: {AutonomousLibrary.Count}");
    }

    private void SaveCurrentAutonomous()
    {
        var name = NewAutonomousRoutineName?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            AddLog("Укажи название автономки.");
            return;
        }

        var routine = new AutonomousRoutine
        {
            Name = name
        };

        foreach (var action in Program.Autonomous)
        {
            var copy = CloneAction(action);
            copy.Parent = null;
            routine.Actions.Add(copy);
        }

        AutonomousLibraryService.Save(routine);
        LoadAutonomousLibrary();

        SelectedAutonomousRoutine = AutonomousLibrary
            .FirstOrDefault(x => x.Name == routine.Name);

        NewAutonomousRoutineName = routine.Name;

        AddLog($"Автономка сохранена в библиотеку: {routine.Name}");
        UpdatePreview();
    }

    private void LoadSelectedAutonomous()
    {
        if (SelectedAutonomousRoutine == null)
        {
            AddLog("Сначала выбери автономку из библиотеки.");
            return;
        }

        Program.Autonomous.Clear();

        foreach (var action in SelectedAutonomousRoutine.Actions)
        {
            var copy = CloneAction(action);
            copy.Parent = null;
            Program.Autonomous.Add(copy);
        }

        RestoreParents(Program.Autonomous);

        NewAutonomousRoutineName = SelectedAutonomousRoutine.Name;
        SelectedAction = null;

        OnPropertyChanged(nameof(Actions));

        AddLog($"Загружена автономка: {SelectedAutonomousRoutine.Name}");
        UpdatePreview();
    }
    private void NewAutonomous()
    {
        Program.Autonomous.Clear();

        SelectedAutonomousRoutine = null;
        SelectedAction = null;
        NewAutonomousRoutineName = "Новая автономка";

        if (ActiveTabIndex != 4) // если Autonomous у тебя на другой вкладке — поменяй индекс
            ActiveTabIndex = 4;

        OnPropertyChanged(nameof(Actions));

        AddLog("Создана новая пустая автономка.");
        UpdatePreview();
    }
    #endregion

    #region Base code / COM ports

    private void AddInclude()
    {
        var include = new ProgramInclude
        {
            Header = "",
            Kind = ProgramIncludeKind.System,
            IsEnabled = true
        };

        Program.Includes.Add(include);
        SelectedInclude = include;

        AddLog($"Добавлен include: {include.GenerateCode()}");
        UpdatePreview();
    }

    private void DeleteInclude()
    {
        if (SelectedInclude == null)
            return;

        Program.Includes.Remove(SelectedInclude);
        SelectedInclude = null;

        AddLog("Include удалён");
        UpdatePreview();
    }

    private void RefreshComPorts()
    {
        var previousPort = SelectedComPort;

        AvailableComPorts.Clear();

        foreach (var port in SerialPort.GetPortNames().OrderBy(port => port))
            AvailableComPorts.Add(port);

        if (previousPort != null && AvailableComPorts.Contains(previousPort))
            SelectedComPort = previousPort;
        else
            SelectedComPort = AvailableComPorts.FirstOrDefault();

        if (AvailableComPorts.Count == 0)
            AddLog("COM-порты не найдены.");
        else
            AddLog($"Найдено COM-портов: {AvailableComPorts.Count}");
    }

    #endregion

    #region Velopack updates check
    private async void CheckUpdates()
    {
        await CheckUpdatesAsync();
    }

    private async Task CheckUpdatesAsync()
    {
        try
        {
            AddLog("[Update] Проверяю обновления...");

            var source = new GithubSource(
                "https://github.com/Chikidrill/RobotProgrammer",
                null,
                false);

            var manager = new UpdateManager(source);

            var update = await manager.CheckForUpdatesAsync();

            if (update == null)
            {
                AddLog("[Update] Обновлений нет.");
                return;
            }

            AddLog($"[Update] Найдена версия: {update.TargetFullRelease.Version}");
            AddLog("[Update] Скачиваю обновление...");

            await manager.DownloadUpdatesAsync(update, progress =>
            {
                AddLog($"[Update] Загрузка: {progress}%");
            });

            AddLog("[Update] Обновление скачано. Приложение будет перезапущено.");

            manager.ApplyUpdatesAndRestart(update);
        }
        catch (Exception ex)
        {
            AddLog("[Update Error] " + ex.Message);
        }
    }
    #endregion
}