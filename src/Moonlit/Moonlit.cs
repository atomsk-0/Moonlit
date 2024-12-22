using Moonlit.Util;

namespace Moonlit;

public static unsafe class Moonlit
{
    public static void Free(void* p) => NativeMemory.Free(p);
}