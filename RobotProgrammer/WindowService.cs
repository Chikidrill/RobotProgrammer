using RobotProgrammer.ViewModel;
using System.Windows;

namespace RobotProgrammer.View
{
    public class WindowService : IWindowService
    {
        public bool? ShowDialog(object viewModel)
        {
            // Создаём окно для шаблона
            var window = new NewTemplateWindow();
            window.DataContext = viewModel;
            return window.ShowDialog();
        }
    }
}
