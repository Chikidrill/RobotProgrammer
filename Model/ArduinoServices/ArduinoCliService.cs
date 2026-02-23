using System.Diagnostics;
using System.Text;

namespace Model.ArduinoServices;
public class ArduinoCliService
{
    private const string ArduinoCliExe = "C:\\Users\\dvorn\\source\\repos\\RobotProgrammerApp\\Model\\arduino-cli.exe";
    public void Compile(string projectPath, Action<string> logCallback)
    {
        Run("compile", $"--fqbn arduino:avr:uno \"{projectPath}\"", logCallback);
    }

    public void Upload(string projectPath, string port, Action<string> logCallback)
    {
        Run("upload", $"--fqbn arduino:avr:uno -p {port} \"{projectPath}\"", logCallback);
    }

    private void Run(string command, string args, Action<string> logCallback)
    {
        var psi = new ProcessStartInfo
        {
            FileName = ArduinoCliExe,
            Arguments = $"{command} {args}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using var process = new Process { StartInfo = psi };

        process.OutputDataReceived += (_, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
                logCallback?.Invoke("[CLI] " + e.Data);
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
                logCallback?.Invoke("[CLI ERROR] " + e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        if (process.ExitCode != 0)
            throw new Exception($"Процесс завершился с кодом {process.ExitCode}");
    }
}

