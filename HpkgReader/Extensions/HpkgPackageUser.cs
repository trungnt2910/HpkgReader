using HpkgReader.Model;
using System.Collections.Generic;
using System.Linq;

namespace HpkgReader.Extensions
{
    public class HpkgPackageUser
    {
        public string Name { get; set; }
        public string RealName { get; set; }
        public string Home { get; set; }
        public string Shell { get; set; } = "/bin/bash";
        public List<string> Groups { get; set; } = new List<string>();

        public override string ToString()
        {
            return $"{Name} " +
                $"{((RealName != null) ? $"real-name {RealName} " : "")}" +
                $"home {Home} " +
                $"{((Shell != null) ? $"shell {Shell} " : "")}" +
                $"{(((Groups?.Count ?? 0) > 0) ? $"groups {{ {string.Join(" ", Groups)} }} " : "")}";
        }

        internal BetterAttribute ToAttribute(AttributeId id = null)
        {
            id = id ?? AttributeId.PACKAGE_USER;
            return new BetterAttribute(id, Name)
                .WithChildIfNotNull(RealName, (r) =>
                    new BetterAttribute(AttributeId.PACKAGE_USER_REAL_NAME, r)
                        .GuessTypeAndEncoding())
                .WithChildIfNotNull(Home, (h) =>
                    new BetterAttribute(AttributeId.PACKAGE_USER_HOME, h)
                        .GuessTypeAndEncoding())
                .WithChildIfNotNull(Shell, (s) =>
                    new BetterAttribute(AttributeId.PACKAGE_USER_SHELL, s)
                        .GuessTypeAndEncoding())
                .WithChildren(Groups.Select(g =>
                    new BetterAttribute(AttributeId.PACKAGE_USER_GROUP, g)
                        .GuessTypeAndEncoding()));
        }
    }
}
