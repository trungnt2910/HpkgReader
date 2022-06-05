using HpkgReader.Compat;
using System;
using System.Text.RegularExpressions;

namespace HpkgReader.Extensions
{
    /// <summary>
    /// <para>A package URL when supplied as a string can be either the URL &quot;naked&quot; or it can be of a form such
    /// as;</para>
    /// <code>Foo Bar &lt;http://www.example.com/&gt;</code>
    /// <para>This class is able to parse those forms and model the resultant name and URL.</para>
    /// </summary>
    public class BetterPkgUrl
    {
        private readonly static Regex PATTERN_NAMEANDURL = new Regex("^([^<>]*)(\\s+)?<([^<> ]+)>$", RegexOptions.Compiled);

        private readonly string _input;
        private readonly string _url;
        private readonly string _name;

        public BetterPkgUrl(string input)
            : base()
        {
            Preconditions.CheckState(!string.IsNullOrEmpty(input));

            input = input.Trim();

            if (string.IsNullOrEmpty(input))
            {
                throw new InvalidOperationException("the input must be supplied and should not be an empty string when trimmed.");
            }

            Match matcher = PATTERN_NAMEANDURL.Match(input);

            if (matcher.Success)
            {
                _url = matcher.Group(3);
                _name = StringCompat.EmptyToNull(matcher.Group(1).Trim());
            }
            else
            {
                _url = input;
                _name = null;
            }

            _input = input;
        }

        public string Url => _url;

        public string Name => _name;

        public override string ToString()
        {
            return _input;
        }
    }
}
