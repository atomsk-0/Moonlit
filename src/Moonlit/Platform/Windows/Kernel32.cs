using System.Runtime.InteropServices;

namespace Moonlit.Platform.Windows;

internal static partial class Kernel32
{
    private const string library = "kernel32.dll";

    [LibraryImport(library)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static unsafe partial bool ReadProcessMemory(nint hProcess, void* lpBaseAddress, void* lpBuffer, nuint nSize, nuint* lpNumberOfBytesRead);
}