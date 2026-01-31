using RobotProgrammer.ViewModel;
using System.Windows;

namespace RobotProgrammer.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainVM(new FileDialogService(), new WindowService());
        }
    }
}
