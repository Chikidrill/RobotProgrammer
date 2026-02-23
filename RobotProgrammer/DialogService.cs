using System.Windows;
using ViewModel;

public class DialogService : IDialogService
{
    public void ShowMessage(string message, string title)
    {
        MessageBox.Show(
            message,
            title,
            MessageBoxButton.OK,
            MessageBoxImage.Warning
        );
    }
}