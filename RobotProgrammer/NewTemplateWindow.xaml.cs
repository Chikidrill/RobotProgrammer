using System.Windows;
using RobotProgrammer.ViewModel;

namespace RobotProgrammer.View
{
    public partial class NewTemplateWindow : Window
    {
        public NewTemplateWindow()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                if (DataContext is NewTemplateVM vm)  // ← здесь был старый класс
                {
                    vm.RequestClose += (sender, dialogResult) =>
                    {
                        DialogResult = dialogResult;
                        Close();
                    };
                }
            };
        }

        // Новый метод для ShowDialog с VM
        public bool? ShowDialog(NewTemplateVM vm)
        {
            DataContext = vm;
            return this.ShowDialog();
        }
    }
}
