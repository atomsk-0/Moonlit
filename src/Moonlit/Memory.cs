using Moonlit.Manager;
using Moonlit.Platform.Windows;
using Moonlit.Util;

namespace Moonlit;

public static unsafe class Memory
{
    private const char wildcard = '?';

    public static byte* BaseAddress;
    public static byte* EndAddress;

    /// <summary>
    /// Converts a string to a byte array
    /// </summary>
    /// <param name="str"></param>
    /// <param name="length"></param>
    /// <returns>pointer to heap allocated byte array. Free it after usage</returns>
    public static byte* StringToBytes(string str, int* length)
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

        byte* buffer = NativeMemory.Alloc<byte>(chunkSize);

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

        NativeMemory.Free(buffer);
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
    /// Reset memory addresses
    /// </summary>
    public static void Reset()
    {
        BaseAddress = null;
        EndAddress = null;
    }
}