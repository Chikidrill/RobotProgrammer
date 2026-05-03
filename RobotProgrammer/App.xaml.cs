using System;
using System.Windows;
using Velopack;

namespace RobotProgrammer;

public partial class App : Application
{
    [STAThread]
    public static void Main(string[] args)
    {
        VelopackApp.Build()
            .Run();

        var app = new App();
        app.InitializeComponent();
        app.Run();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        AppPaths.EnsureCreated();

        base.OnStartup(e);
    }
}