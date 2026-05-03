# Robot Programmer

**Robot Programmer** — WPF-приложение для визуального программирования робота на Arduino/PRIZM.  
Приложение позволяет собирать программу из блоков, настраивать автономный и телеуправляемый режимы, создавать пользовательские функции и шаблоны, просматривать сгенерированный Arduino-код, компилировать и загружать его на контроллер.

## Возможности

- Визуальная сборка программы из блоков.
- Разделение программы на:
  - `Base Code`
  - `Setup`
  - `Autonomous`
  - `Teleop`
  - `Functions`
  - `Templates`
  - `Variables`
- Поддержка автономного режима.
- Поддержка телеуправления через PS4-контроллер.
- Правила кнопок для Teleop:
  - действие при удержании;
  - действие при нажатии;
  - действие при отпускании;
  - переключаемые режимы.
- Пользовательские функции.
- Библиотека функций.
- Пользовательские блоки/шаблоны.
- Библиотека автономок.
- Редактируемые `#include`.
- Редактируемый базовый код:
  - глобальные объявления;
  - базовый `setup`;
  - базовый `loop`;
  - завершающий код `loop`.
- Предпросмотр итогового `.ino`-кода.
- Компиляция через Arduino CLI.
- Загрузка на выбранный COM-порт.
- Drag & Drop для дерева действий.
- Контекстные меню для добавления, копирования, вставки и удаления блоков.

## Технологии

- C#
- WPF
- MVVM-подход
- .NET
- Arduino CLI
- System.Text.Json
- System.IO.Ports
- Arduino/PRIZM libraries

## Структура проекта

```text
RobotProgrammerApp/
│
├── RobotProgrammer/          # WPF View, MainWindow.xaml
│
├── ViewModel/                # ViewModel-слой
│   ├── MainVM.cs
│   ├── RelayCommand.cs
│   └── ...
│
├── Model/                    # Модель и генерация кода
│   ├── RobotActions/
│   │   ├── RobotAction.cs
│   │   ├── MoveAction.cs
│   │   ├── WaitAction.cs
│   │   ├── LoopAction.cs
│   │   ├── ConditionalAction.cs
│   │   ├── BranchAction.cs
│   │   ├── CustomAction.cs
│   │   ├── FunctionCallAction.cs
│   │   └── ...
│   │
│   ├── ArduinoServices/
│   │   ├── ArduinoCodeGenerator.cs
│   │   └── ArduinoCliService.cs
│   │
│   ├── Services/
│   │   ├── TemplateService.cs
│   │   ├── FunctionLibraryService.cs
│   │   ├── AutonomousLibraryService.cs
│   │   ├── ProjectFileSaving.cs
│   │   └── ...
│   │
│   ├── RobotProgram.cs
│   ├── ProgramVariable.cs
│   ├── ProgramFunction.cs
│   ├── ProgramInclude.cs
│   └── ...
│
└── README.md
