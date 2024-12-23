using Moonlit.Manager;
using Moonlit.Platform.Windows;
using Moonlit.Util;

namespace Moonlit;

public static unsafe class Memory
{
    private const char wildcard = '?';
    private const int max_stackalloc = 1024;

    public static byte* BaseAddress;
    public static byte* EndAddress;

    /// <summary>
    /// Converts a hex string to a byte array
    /// </summary>
    /// <param name="str"></param>
    /// <param name="length"></param>
    /// <returns>pointer to heap allocated byte array. Free it after usage</returns>
    public static byte* HexStringToBytes(string str, int* length)
    {
        str = str.Replace(" ", ""); // Remove spaces
        *length = str.Length / 2;
        byte* bytes = NativeMemory.Alloc<byte>(*length); // Heap allocation, remember to free it after use
        for (int i = 0; i < *length; i++)
        {
            string hexPair = str.Substring(i * 2, 2);
            bytes[i] = Convert.ToByte(hexPair, 16);
        }

        return bytes;
    }

    /// <summary>
    /// Converts a string to a byte array.
    /// </summary>
    /// <param name="str">The input string to convert.</param>
    /// <param name="length">Pointer to an integer that will store the length of the resulting byte array.</param>
    /// <returns>
    /// A pointer to a heap-allocated byte array representing the input string.
    /// The caller is responsible for freeing the allocated memory after use.
    /// </returns>
    public static byte* StringToBytes(string str, int* length)
    {
        byte* bytes = NativeMemory.Alloc<byte>((nuint)str.Length);
        for (int i = 0; i < str.Length; i++)
        {
            bytes[i] = (byte)str[i];
        }

        *length = str.Length;
        return bytes;
    }


    /// <summary>
    /// Converts a pattern to a byte array
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="length"></param>
    /// <returns>pointer to heap allocated byte array. Free it after usage</returns>
    public static int* PatternToBytes(string pattern, int* length)
    {
        pattern = pattern.Replace(" ", ""); // Remove spaces
        *length = pattern.Length / 2;
        int* bytes = NativeMemory.Alloc<int>(*length); // Heap allocation, remember to free it after use

        for (int i = 0; i < *length; i++)
        {
            string hexPair = pattern.Substring(i * 2, 2);
            bytes[i] = hexPair[0] == wildcard || hexPair[1] == wildcard ? -1 : Convert.ToInt32(hexPair, 16);
        }

        return bytes;
    }


    /// <summary>
    /// Finds a pattern in a specified memory range.
    /// </summary>
    /// <param name="pattern">The pattern to search for, represented as a string.</param>
    /// <param name="rangeStart">The starting address of the memory range to search.</param>
    /// <param name="rangeEnd">The ending address of the memory range to search.</param>
    /// <param name="chunkSize">The size of the chunks to read from memory.</param>
    /// <param name="outLength">The output parameter that will contain the number of matches found.</param>
    /// <returns>
    /// A pointer to an array of byte pointers, each pointing to the start of a match in memory.
    /// Returns null if no matches are found.
    /// </returns>
    public static byte** FindPattern(string pattern, byte* rangeStart, byte* rangeEnd, int chunkSize, out int outLength)
    {
        int fixedLength;
        int* bytes = PatternToBytes(pattern, &fixedLength);
        int length = fixedLength;

        List<nuint> patternBytes = new List<nuint>();
        byte* buffer;
        if (chunkSize <= max_stackalloc)
        {
            byte* stackBuffer = stackalloc byte[chunkSize];
            buffer = stackBuffer;
        }
        else
        {
            buffer = NativeMemory.Alloc<byte>(chunkSize);
        }

        for (byte* i = rangeStart; i <= rangeEnd - length; i += chunkSize)
        {
            nuint bytesRead = 0;
            if (OperatingSystem.IsWindows())
            {
                Kernel32.ReadProcessMemory(ProcessManager.CurrentProcess!.Handle, i, buffer, (nuint)chunkSize, &bytesRead);
            }

            if (bytesRead == 0) break;

            byte* i1 = i;
            Parallel.For(0, (int)bytesRead - length, offset =>
            {
                for (int j = 0; j < length; j++)
                {
                    if (buffer[offset + j] != bytes[j] && bytes[j] != -1) return;
                    if (j == length - 1) lock (patternBytes) patternBytes.Add((nuint)(i1 + offset));
                }
            });
        }

        if (chunkSize > max_stackalloc)
        {
            NativeMemory.Free(buffer);
        }
        NativeMemory.Free(bytes);

       outLength = patternBytes.Count;

        if (patternBytes.Count > 0)
        {
            byte** result = (byte**)NativeMemory.Alloc<nint>((nuint)patternBytes.Count);
            for (int i = 0; i < patternBytes.Count; i++)
            {
                result[i] = (byte*)patternBytes[i];
            }

            return result;
        }

        return null;
    }

    /// <summary>
    /// Finds the specified text within a given memory range.
    /// </summary>
    /// <param name="text">The text to search for.</param>
    /// <param name="rangeStart">The starting address of the memory range to search.</param>
    /// <param name="rangeEnd">The ending address of the memory range to search.</param>
    /// <returns>
    /// A pointer to the start of the found text within the memory range, or null if the text is not found.
    /// </returns>
    public static byte* FindText(string text, byte* rangeStart, byte* rangeEnd)
    {
        int fixedLength;
        byte* bytes = StringToBytes(text, &fixedLength);
        int length = fixedLength;

        const int chunk_size = 8192; // 8KB - maybe change this as parameter in the function
        byte* buffer = NativeMemory.Alloc<byte>(chunk_size);

        for (byte* i = rangeStart; i <= rangeEnd - length; i += chunk_size)
        {
            nuint bytesRead = 0;
            if (OperatingSystem.IsWindows())
            {
                Kernel32.ReadProcessMemory(ProcessManager.CurrentProcess!.Handle, i, buffer, chunk_size, &bytesRead);
            }

            if (bytesRead == 0) break;

            bool found = false;
            Parallel.For(0, (int)bytesRead - length, (offset, state) =>
            {
                if (NativeMemory.MemCmp(buffer + offset, bytes, length) == 0)
                {
                    found = true;
                    state.Stop();
                }
            });

            if (found)
            {
                NativeMemory.Free(bytes);
                NativeMemory.Free(buffer);
                return i;
            }
        }

        NativeMemory.Free(bytes);
        NativeMemory.Free(buffer);
        return null;
    }

    /// <summary>
    /// Reset memory addresses
    /// </summary>
    public static void Reset()
    {
        BaseAddress = null;
        EndAddress = null;
    }
}