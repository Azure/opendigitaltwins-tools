using System;

namespace ADTToolsLibrary
{
    public static class Log
    {
        static public void Write(string s, ConsoleColor col = ConsoleColor.White)
        {
            Console.ForegroundColor = col;
            Console.WriteLine(s);
            Console.ResetColor();
        }

        static public void Error(string s)
        {
            Write(s, ConsoleColor.DarkRed);
        }

        static public void Warning(string s)
        {
            Write(s, ConsoleColor.DarkYellow);
        }

        static public void Ok(string s)
        {
            Write(s, ConsoleColor.DarkGreen);
        }
    }
}
