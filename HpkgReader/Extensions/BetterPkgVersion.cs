using HpkgReader.Model;
using System.Text;

namespace HpkgReader.Extensions
{
    /// <summary>
    /// PkgVersion with a better ToString method and mutable members
    /// </summary>
    public class BetterPkgVersion
    {
        public string Major { get; set; }
        public string Minor { get; set; }
        public string Micro { get; set; }
        public string PreRelease { get; set; }
        public int? Revision { get; set; }

        public static explicit operator PkgVersion(BetterPkgVersion betterVersion)
        {
            return new PkgVersion(
                betterVersion.Major,
                betterVersion.Minor,
                betterVersion.Micro,
                betterVersion.PreRelease,
                betterVersion.Revision);
        }

        /// <summary>
        /// Returns a string that represents the stored version.
        /// </summary>
        /// <returns>A string that represents the stored version</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Major);
            if (!string.IsNullOrEmpty(Minor))
            {
                sb.Append('.');
                sb.Append(Minor);
            }
            if (!string.IsNullOrEmpty(Micro))
            {
                sb.Append('.');
                sb.Append(Micro);
            }
            // PreRelease version tags are often
            // prefixed with -
            // For example, 0.0.1-dev.1
            if (!string.IsNullOrEmpty(PreRelease))
            {
                sb.Append('-');
                sb.Append(PreRelease);
            }
            // Hpkg recipe revision.
            if (Revision != null)
            {
                sb.Append('-');
                sb.Append((int)Revision);
            }
            return sb.ToString();
        }
    }
}
