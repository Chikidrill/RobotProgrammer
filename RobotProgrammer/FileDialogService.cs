using Microsoft.Win32;
using RobotProgrammer.ViewModel;

namespace RobotProgrammer.View;

public class FileDialogService : IFileDialogService
{
    public string SaveFile(string filter, string defaultExt)
    {
        var dialog = new SaveFileDialog
        {
            Filter = filter,
            DefaultExt = defaultExt
        };

        return dialog.ShowDialog() == true
            ? dialog.FileName
            : null;
    }

    public string OpenFile(string filter)
    {
        var dialog = new OpenFileDialog
        {
            Filter = filter
        };

        return dialog.ShowDialog() == true
            ? dialog.FileName
            : null;
    }
}
