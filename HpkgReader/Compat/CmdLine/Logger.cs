using System;
using System.Collections.Generic;
using System.Text;

namespace HpkgReader.Compat.CmdLine
{
    public class Logger
    {
        private readonly Type _type;

        internal Logger(Type type)
        {
            _type = type;
        }

        public void Error(string message)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.Write("[ERROR]");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Error.Write($" {_type.FullName}: ");
            Console.ForegroundColor = oldColor;
            Console.Error.WriteLine(message);
        }

        public void Error(string message, object obj)
        {
            Error($"{message}: {obj}");
        }
    }
}
