using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Model.ArduinoServices;

public class ArduinoCliService
{
    private const string Fqbn = "arduino:avr:uno";

    private static readonly TimeSpan DefaultCompileTimeout = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan DefaultUploadTimeout = TimeSpan.FromSeconds(45);

    private readonly string _cliPath = Path.Combine(
        AppContext.BaseDirectory,
        "Tools",
        "arduino-cli.exe");

    public Task CompileAsync(
        string projectPath,
        IProgress<string> progress,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        EnsureCliExists(progress);

        string arguments = $"compile --fqbn {Fqbn} \"{projectPath}\"";

        return RunCliAsync(
            arguments,
            progress,
            timeout ?? DefaultCompileTimeout,
            cancellationToken);
    }

    public Task UploadAsync(
        string projectPath,
        string port,
        IProgress<string> progress,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        EnsureCliExists(progress);

        string arguments = $"upload -p {port} --fqbn {Fqbn} \"{projectPath}\"";

        return RunCliAsync(
            arguments,
            progress,
            timeout ?? DefaultUploadTimeout,
            cancellationToken);
    }

    private void EnsureCliExists(IProgress<string> progress)
    {
        progress.Report("[CLI PATH] " + _cliPath);
        progress.Report("[CLI EXISTS] " + File.Exists(_cliPath));

        if (!File.Exists(_cliPath))
        {
            throw new FileNotFoundException(
                "arduino-cli.exe не найден по пути: " + _cliPath);
        }
    }

    private async Task RunCliAsync(
        string arguments,
        IProgress<string> progress,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _cliPath,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using var process = new Process
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true
        };

        progress.Report("[CLI] " + _cliPath + " " + arguments);

        Task outputTask = Task.CompletedTask;
        Task errorTask = Task.CompletedTask;

        try
        {
            if (!process.Start())
                throw new InvalidOperationException("Не удалось запустить arduino-cli.");

            outputTask = ReadLinesAsync(process.StandardOutput, progress);
            errorTask = ReadLinesAsync(process.StandardError, progress);

            await process
                .WaitForExitAsync(cancellationToken)
                .WaitAsync(timeout, cancellationToken);

            await Task.WhenAll(outputTask, errorTask);

            if (process.ExitCode != 0)
            {
                throw new Exception(
                    $"Arduino CLI завершился с кодом {process.ExitCode}");
            }
        }
        catch (TimeoutException)
        {
            progress.Report($"[CLI TIMEOUT] Команда выполнялась дольше {timeout.TotalSeconds:0} сек.");
            progress.Report("[CLI TIMEOUT] Останавливаю arduino-cli...");

            KillProcessTree(process);

            try
            {
                await Task.WhenAll(outputTask, errorTask);
            }
            catch
            {
                // После kill потоки вывода могут закрыться с исключением — это нормально.
            }

            throw new TimeoutException(
                $"Arduino CLI завис или слишком долго ждал устройство. Команда остановлена через {timeout.TotalSeconds:0} сек.");
        }
        catch (OperationCanceledException)
        {
            progress.Report("[CLI CANCELLED] Операция отменена.");

            KillProcessTree(process);

            throw;
        }
    }

    private static async Task ReadLinesAsync(
        StreamReader reader,
        IProgress<string> progress)
    {
        while (!reader.EndOfStream)
        {
            string? line = await reader.ReadLineAsync();

            if (!string.IsNullOrWhiteSpace(line))
                progress.Report(line);
        }
    }

    private static void KillProcessTree(Process process)
    {
        try
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
        }
        catch
        {
            // Процесс мог уже завершиться сам.
        }
    }
}