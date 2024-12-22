using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Moonlit.Platform.Linux;

internal static partial class Libc
{
    internal enum PtraceRequest : int
    {
        PtraceAttach = 16,
        PtraceDetach = 17,
        PtracePeekdata = 2
    }

    private const string library = "libc.so.6";

    [LibraryImport(library)]
    internal static unsafe partial long ptrace(PtraceRequest request, int pid, void* addr, void* data);

    [LibraryImport(library)]
    internal static unsafe partial int waitpid(int pid, int* status, int options);
}