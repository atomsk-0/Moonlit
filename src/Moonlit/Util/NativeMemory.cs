using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace Moonlit.Util;

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal static unsafe class NativeMemory
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T* Alloc<T>(nuint size) where T : unmanaged => (T*)System.Runtime.InteropServices.NativeMemory.Alloc(size * (nuint)sizeof(T));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T* Alloc<T>(int size) where T : unmanaged => (T*)System.Runtime.InteropServices.NativeMemory.Alloc((nuint)size * (nuint)sizeof(T));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T* Alloc<T>(uint size) where T : unmanaged => (T*)System.Runtime.InteropServices.NativeMemory.Alloc((nuint)size * (nuint)sizeof(T));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Free(void* ptr) => System.Runtime.InteropServices.NativeMemory.Free(ptr);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetByteCountUTF8(string str)
    {
        return Encoding.UTF8.GetByteCount(str);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetByteCountUTF16(string str)
    {
        return Encoding.Unicode.GetByteCount(str);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static sbyte* StringToUTF8Ptr(string? str)
    {
        if (str == null) return null;
        int size = GetByteCountUTF8(str);
        byte* ptr = Alloc<byte>((nuint)(size + 1));
        fixed (char* pStr = str)
        {
            Encoding.UTF8.GetBytes(pStr, str.Length, ptr, size);
        }
        ptr[size] = 0;
        return (sbyte*)ptr;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static char* StringToUTF16Ptr(string? str)
    {
        if (str == null) return null;
        int size = GetByteCountUTF16(str);
        char* ptr = Alloc<char>((nuint)(size + 1));
        fixed (char* pStr = str)
        {
            Encoding.Unicode.GetBytes(pStr, str.Length, (byte*)ptr, size);
        }
        ptr[size] = (char)0;
        return ptr;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string? PtrToUTF8String(byte* ptr)
    {
        if (ptr == null) return null;
        int length = 0;
        while (ptr[length] != 0) length++;
        return Encoding.UTF8.GetString(ptr, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string? PtrToUTF16String(char* ptr)
    {
        if (ptr == null) return null;
        int length = 0;
        while (ptr[length] != 0) length++;
        return new string(ptr, 0, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Memcpy(void* dest, void* src, nuint size)
    {
        byte* pDest = (byte*)dest;
        byte* pSrc = (byte*)src;
        for (nuint i = 0; i < size; i++)
        {
            pDest[i] = pSrc[i];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int MemCmp(void* buf1, void* buf2, int size)
    {
        byte* pBuf1 = (byte*)buf1;
        byte* pBuf2 = (byte*)buf2;
        for (int i = 0; i < size; i++)
        {
            if (pBuf1[i] != pBuf2[i])
            {
                return pBuf1[i] - pBuf2[i];
            }
        }
        return 0;
    }
}