using Moonlit.Manager;

namespace Moonlit.Lab;

internal static unsafe class Program
{
    private static void Main()
    {
        if (ProcessManager.Open("Growtopia"))
        {
            Console.WriteLine("Process opened successfully");
            Console.WriteLine("Base address: 0x{0:X}", (ulong)Memory.BaseAddress);
            Console.WriteLine("End address: 0x{0:X}", (ulong)Memory.EndAddress);
            Console.WriteLine("Searching for pattern: ?? 66 ?? F7 ?? ??");
            byte** patterns = Memory.FindPattern("?? 66 ?? F7 ?? ??", Memory.BaseAddress, Memory.EndAddress, 4096, out int length);
            Console.WriteLine("Found: {0}", length);
            for (int i = 0; i < length; i++)
            {
                Console.WriteLine("0x{0:X}", (ulong)patterns[i]);
            }
            Moonlit.Free(patterns);
        }
        else
        {
            Console.WriteLine("Failed to open process");
        }
        Console.ReadKey();
    }
}