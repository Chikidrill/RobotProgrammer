
public static class AppPaths
{
#if DEBUG
    public const string AppName = "RobotProgrammer.Dev";
#else
    public const string AppName = "RobotProgrammer";
#endif


    public static string RoamingAppData =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            AppName);

    public static string LocalAppData =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            AppName);

    public static string Logs =>
        Path.Combine(LocalAppData, "logs");

    public static string Temp =>
        Path.Combine(LocalAppData, "temp");

    public static string Backups =>
        Path.Combine(LocalAppData, "backups");

    public static string SettingsFile =>
        Path.Combine(RoamingAppData, "settings.json");

    public static string RecentProjectsFile =>
        Path.Combine(RoamingAppData, "recent-projects.json");

    public static string DefaultProjectsFolder =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            AppName,
            "Projects");

    public static void EnsureCreated()
    {
        Directory.CreateDirectory(RoamingAppData);
        Directory.CreateDirectory(LocalAppData);
        Directory.CreateDirectory(Logs);
        Directory.CreateDirectory(Temp);
        Directory.CreateDirectory(Backups);
        Directory.CreateDirectory(DefaultProjectsFolder);
    }


}