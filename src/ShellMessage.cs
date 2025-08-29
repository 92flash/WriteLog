using System;
using static WriteLog.Color;

namespace WriteLog
{
    class ShellMessage
    {

        public ShellMessage()
        {
            
        }

        // Write a message to the console with optional color
        public static void Write(string message, Colors color = Colors.Default, bool newLine = true)
        {
            Color colorCode = new(color);
            if (newLine)
            {
                Console.WriteLine($"{colorCode.Code}{message}{colorCode.Reset}");
            }
            else
            {
                Console.Write($"{colorCode.Code}{message}{colorCode.Reset}");
            }
        }
    }
}