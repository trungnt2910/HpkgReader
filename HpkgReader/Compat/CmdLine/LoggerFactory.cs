using System;
using System.Collections.Generic;

namespace HpkgReader.Compat.CmdLine
{
    internal static class LoggerFactory
    {
        private static Dictionary<Type, Logger> _loggers = new Dictionary<Type, Logger>();

        public static Logger GetLogger(Type type)
        {
            lock (_loggers)
            {
                if (_loggers.TryGetValue(type, out var logger))
                {
                    return logger;
                }
                logger = new Logger(type);
                _loggers.Add(type, logger);
                return logger;
            }
        }
    }
}
