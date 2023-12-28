using System;
using System.Threading.Tasks;

namespace rommelrouterakkers;

public class Program
{ 
    static void Main()
    {
        Setup s = new Setup();
        //Task keyHandlerTask = Task.Run(() => HandleKeyEvents(s)); //kost te veel iteraties
        s.ILS();
        Console.ReadLine();
    }

    static void HandleKeyEvents(Setup s)
    {
        while (true)
        {
            // Read a key without displaying it on the console
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            // Check the pressed key
            if (keyInfo.Key == ConsoleKey.Q)
            {
                s.ScreenShot();
            }
            else
            {
                Console.WriteLine($"You pressed: {keyInfo.KeyChar}");
            }
        }
    }
}