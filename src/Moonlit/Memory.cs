using System.Runtime.InteropServices;

namespace Moonlit;

public static unsafe class Memory
{
    public static byte* BaseAddress;
    public static byte* EndAddress;

    public static byte* StringToBytes(string str, uint* length)
    {
        // Remove spaces
        str = str.Replace(" ", "");
        *length = (uint)str.Length / 2;
        byte* bytes = (byte*)NativeMemory.Alloc(*length); // Heap allocation, remember to free it after use
        for (uint i = 0; i < *length; i++)
        {
            string hexPair = str.Substring((int)(i * 2), 2);
            bytes[i] = byte.Parse(hexPair, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
        }

        return bytes;
    }
}