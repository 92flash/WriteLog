using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Microsoft.PowerShell.Commands;
using static WriteLog.Color;

namespace WriteLog
{
    class ShellMessage
    {

        public ShellMessage()
        {
            
        }

        // Write a message to the console with optional color
        public static void Write(string message, Colors color = Colors.Default, bool newLine = true, PSCmdlet context = null)
        {
            Color colorCode = new(color);
            if (newLine)
            {
                context?.WriteObject($"{colorCode.Code}{message}{colorCode.Reset}");
            }
            else
            {
                Console.Write($"{colorCode.Code}{message}{colorCode.Reset}");
            }
        }
    }
}