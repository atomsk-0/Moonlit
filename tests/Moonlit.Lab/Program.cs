﻿using Moonlit.Manager;

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
            byte* textAddress = Memory.FindText("Internal memory error 49", Memory.BaseAddress, Memory.EndAddress);
            Console.WriteLine("Found text at: 0x{0:X}", (ulong)textAddress);
        }
        else
        {
            Console.WriteLine("Failed to open process");
        }
        Console.ReadKey();
    }
}