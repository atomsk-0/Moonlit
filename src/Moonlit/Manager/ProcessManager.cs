using System.Diagnostics;

namespace Moonlit.Manager;

public static unsafe class ProcessManager
{
    /// <summary>
    /// Currently opened process
    /// </summary>
    public static Process? CurrentProcess;

    /// <summary>
    /// Open process by name
    /// </summary>
    /// <param name="name">name of process</param>
    /// <returns>true if successful</returns>
    public static bool Open(string name)
    {
        CurrentProcess?.Dispose();
        var processes = Process.GetProcessesByName(name);
        if (processes.Length == 0) return false;
        CurrentProcess = processes[0];
        return true;
    }

    public static bool Open(int id)
    {
        CurrentProcess?.Dispose();
        try
        {
            CurrentProcess = Process.GetProcessById(id);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    /// <summary>
    /// Get all processes
    /// </summary>
    /// <returns>Array of processes</returns>
    public static Process[] GetProcesses()
    {
        return Process.GetProcesses();
    }

    /// <summary>
    /// Get all processes with main window handle
    /// </summary>
    /// <returns>Array of processes</returns>
    public static Process[] GetWindows()
    {
        return Process.GetProcesses().Where(p => p.MainWindowHandle != IntPtr.Zero).ToArray();
    }

    /// <summary>
    /// Get all application processes
    /// </summary>
    /// <returns>Array of processes</returns>
    public static Process[] GetApplications()
    {
        return Process.GetProcesses().Where(p => !string.IsNullOrEmpty(p.MainWindowTitle)).ToArray();
    }
}