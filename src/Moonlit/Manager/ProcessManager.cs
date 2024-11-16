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
        var processes = Process.GetProcessesByName(name);
        return processes.Length != 0 && Open(processes[0].Id);
    }

    public static bool Open(int id)
    {
        CurrentProcess?.Dispose();
        Memory.Reset();
        try
        {
            CurrentProcess = Process.GetProcessById(id);
            var mainModule = CurrentProcess.MainModule;
            if (mainModule == null) return false;
            // Set memory addresses
            Memory.BaseAddress = (byte*)mainModule.BaseAddress;
            Memory.EndAddress = Memory.BaseAddress + mainModule.ModuleMemorySize;
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