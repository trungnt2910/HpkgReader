/*
 * Copyright 2018-2022, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

using HpkgReader.Compat;
using System;
using System.Text.RegularExpressions;

namespace HpkgReader.Model
{
    /// <summary>
    /// <para>A package URL when supplied as a string can be either the URL &quot;naked&quot; or it can be of a form such
    /// as;</para>
    /// <code>Foo Bar &lt;http://www.example.com/&gt;</code>
    /// <para>This class is able to parse those forms and model the resultant name and URL.</para>
    /// </summary>
    public class PkgUrl
    {
        private readonly static Regex PATTERN_NAMEANDURL = new Regex("^([^<>]*)(\\s+)?<([^<> ]+)>$", RegexOptions.Compiled);

        private readonly string url;

        private readonly string name;

        private readonly PkgUrlType urlType;

        public PkgUrl(string input, PkgUrlType urlType)
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
                this.url = matcher.Group(3);
                this.name = StringCompat.EmptyToNull(matcher.Group(1).Trim());
            }
            else
            {
                this.url = input;
                this.name = null;
            }

            this.urlType = urlType;
        }

        public PkgUrlType UrlType => urlType;

        public string Url => url;

        public string Name => name;

        public override string ToString()
        {
            return string.Format("{0}; {1}", urlType.ToString(), url);
        }
    }
}
