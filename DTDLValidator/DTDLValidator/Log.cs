namespace DTDLValidator
{
    using System;

    public static class Log
    {
        static public void Out(string s, ConsoleColor col = ConsoleColor.White)
        {
            Console.ForegroundColor = col;
            Console.WriteLine(s);
            Console.ForegroundColor = ConsoleColor.White;
        }

        static public void Error(string s)
        {
            Out(s, ConsoleColor.DarkRed);
        }

        static public void Alert(string s)
        {
            Out(s, ConsoleColor.DarkYellow);
        }

        static public void Ok(string s)
        {
            Out(s, ConsoleColor.DarkGreen);
        }

        static public void Error(Exception ex, string s)
        {
            var exception = s + "\n" + ex.ToString();
            Out(exception, ConsoleColor.DarkRed);
        }
    }
}
