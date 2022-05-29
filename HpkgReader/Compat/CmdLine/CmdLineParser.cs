using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace HpkgReader.Compat.CmdLine
{
    internal class CmdLineParser
    {
        private object _o;
        public CmdLineParser(object o)
        {
            _o = o;
        }

        public void ParseArgument(string[] args)
        {
            var switches = _o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(fi => fi.GetCustomAttribute(typeof(OptionAttribute)) != null)
                .ToDictionary(fi => fi.GetCustomAttribute<OptionAttribute>().Name, fi => fi);

            var requiredSwitches = switches.Where(kvp => kvp.Value.GetCustomAttribute<OptionAttribute>().Required)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            for (int i = 0; i < args.Length; i++)
            {
                if (switches.TryGetValue(args[i], out var field))
                {
                    if (i + 1 >= args.Length)
                    {
                        throw new CmdLineException($"No value provided for parameter {field.GetCustomAttribute<OptionAttribute>().Name}.");
                    }
                    if (field.FieldType == typeof(FileInfo))
                    {
                        field.SetValue(_o, new FileInfo(args[i + 1]));
                    }
                    else
                    {
                        field.SetValue(_o, args[i + 1]);
                    }
                    requiredSwitches.Remove(args[i]);
                    ++i;
                }
            }

            if (requiredSwitches.Count != 0)
            {
                throw new CmdLineException($"Parameter {requiredSwitches.First().Key} is required: {requiredSwitches.First().Value.GetCustomAttribute<OptionAttribute>().Usage}");
            }
        }
    }
}
