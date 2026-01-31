namespace RobotProgrammer.ViewModel;

public interface IFileDialogService
{
    string SaveFile(string filter, string defaultExt);
    string OpenFile(string filter);
}
