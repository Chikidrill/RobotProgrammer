using System.Diagnostics;
using System.Text;

namespace Model.ArduinoServices;

public class ArduinoCliService
{
    private readonly string _cliPath =
    Path.Combine(AppContext.BaseDirectory, "Tools", "arduino-cli.exe");

    public void Compile(string projectPath, Action<string> logCallback)
    {
        Run(
            command: "compile",
            logCallback: logCallback,
            "--fqbn",
            "arduino:avr:uno",
            projectPath
        );
    }

    public void Upload(string projectPath, string port, Action<string> logCallback)
    {
        Run(
            command: "upload",
            logCallback: logCallback,
            "--fqbn",
            "arduino:avr:uno",
            "-p",
            port,
            projectPath
        );
    }

    private void Run(string command, Action<string> logCallback, params string[] args)
    {
        logCallback?.Invoke($"[CLI PATH] {_cliPath}");
        logCallback?.Invoke($"[CLI EXISTS] {File.Exists(_cliPath)}");

        if (!File.Exists(_cliPath))
        {
            throw new FileNotFoundException(
                $"arduino-cli.exe не найден по пути: {_cliPath}"
            );
        }

        var psi = new ProcessStartInfo
        {
            FileName = _cliPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        psi.ArgumentList.Add(command);

        foreach (var arg in args)
        {
            psi.ArgumentList.Add(arg);
        }

        using var process = new Process
        {
            StartInfo = psi,
            EnableRaisingEvents = true
        };

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

        try
        {
            process.Start();
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"Не удалось запустить arduino-cli.exe. Путь: {_cliPath}",
                ex
            );
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new Exception($"arduino-cli завершился с кодом {process.ExitCode}");
        }
    }
}