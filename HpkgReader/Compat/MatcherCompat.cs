using System.Text.RegularExpressions;

namespace HpkgReader.Compat
{
    internal static class MatcherCompat
    {
        public static string Group(this Match match, int index)
        {
            return match.Groups[index].Value;
        }
    }
}
