/*
 * Copyright 2018, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

using HpkgReader.Compat;
using System.Text;

namespace HpkgReader.Model
{
    public class PkgVersion
    {

        private readonly string major;
        private readonly string minor;
        private readonly string micro;
        private readonly string preRelease;
        private readonly int? revision;

        public PkgVersion(string major, string minor, string micro, string preRelease, int? revision)
        {
            Preconditions.CheckState(!string.IsNullOrEmpty(major));
            this.major = major;
            this.minor = minor;
            this.micro = micro;
            this.preRelease = preRelease;
            this.revision = revision;
        }

        public string Major => major;

        public string Minor => minor;

        public string Micro => micro;

        public string PreRelease => preRelease;

        public int? Revision => revision;

        private void AppendDotValue(StringBuilder stringBuilder, string value)
        {
            if (0 != stringBuilder.Length)
            {
                stringBuilder.Append('.');
            }

            if (null != value)
            {
                stringBuilder.Append(value);
            }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            AppendDotValue(result, Major);
            AppendDotValue(result, Minor);
            AppendDotValue(result, Micro);
            AppendDotValue(result, PreRelease);
            AppendDotValue(result, null == Revision ? null : Revision.ToString());
            return result.ToString();
        }

    }

}