using System;
using System.Collections.Generic;
using System.Text;

namespace HpkgReader.Compat.CmdLine
{
    [AttributeUsage(AttributeTargets.Field)]
    internal class OptionAttribute: Attribute
    {
        public string Name { get; set; }
        public bool Required { get; set; }
        public string Usage { get; set; }
    }
}
