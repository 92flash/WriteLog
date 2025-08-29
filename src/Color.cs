using System;
using System.Security.Cryptography.X509Certificates;

namespace WriteLog
{
    public class Color
    {

        public enum Colors
        {
            Black,
            DarkRed,
            DarkGreen,
            DarkYellow,
            DarkBlue,
            DarkMagenta,
            DarkCyan,
            DarkGray,
            White,
            Gray,
            Red,
            Green,
            Yellow,
            Blue,
            Magenta,
            Cyan,
            BrightWhite,
            Default
        }

        private Colors ColorName { get; set; }
        private string ColorCode { get; set; }

        public Color(Colors colorName)
        {
            ColorName = colorName;
            ColorCode = GetColorCode(ColorName);
        }
        // Use a property to get the color code string
        public string Code => ColorCode;

        public string Reset => "\u001b[0m";

        // Get the ANSI color code based on the color name
        private static string GetColorCode(Colors Name)
        {
            return Name switch
            {
                Colors.Black => "\u001b[30m",
                Colors.DarkRed => "\u001b[31m",
                Colors.DarkGreen => "\u001b[32m",
                Colors.DarkYellow => "\u001b[33m",
                Colors.DarkBlue => "\u001b[34m",
                Colors.DarkMagenta => "\u001b[35m",
                Colors.DarkCyan => "\u001b[36m",
                Colors.DarkGray => "\u001b[90m",
                Colors.White => "\u001b[97m",
                Colors.Gray => "\u001b[37m",
                Colors.Red => "\u001b[91m",
                Colors.Green => "\u001b[92m",
                Colors.Yellow => "\u001b[93m",
                Colors.Blue => "\u001b[94m",
                Colors.Magenta => "\u001b[95m",
                Colors.Cyan => "\u001b[96m",
                Colors.BrightWhite => "\u001b[97m",
                _ => "\u001b[0m", // Default to reset if color not found
            };
        }
    }
}