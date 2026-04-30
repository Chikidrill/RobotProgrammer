using System.Configuration;
using System.Data;
using System.Windows;

namespace RobotProgrammer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AppPaths.EnsureCreated();

            base.OnStartup(e);
        }
    }

}
