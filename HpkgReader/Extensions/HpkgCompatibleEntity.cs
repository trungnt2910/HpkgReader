using HpkgReader.Model;

namespace HpkgReader.Extensions
{
    /// <summary>
    /// An entity with a version and a compatible version.
    /// </summary>
    public class HpkgCompatibleEntity
    {
        public string Name { get; set; }
        public BetterPkgVersion Version { get; set; }
        public BetterPkgVersion CompatibleVersion { get; set; }

        public override string ToString()
        {
            // This is the HaikuPorts format.
            string result = Name;
            if (Version != null)
            {
                result += $" = {Version}";
            }
            if (CompatibleVersion != null)
            {
                result += $" compat {CompatibleVersion}";
            }
            return result;
        }

        internal BetterAttribute ToAttribute(AttributeId id = null)
        {
            id = id ?? AttributeId.PACKAGE_PROVIDES;
            return new BetterAttribute(id, Name)
                .GuessTypeAndEncoding()
                .WithChild(new BetterAttribute(Version))
                .WithChildIfNotNull(CompatibleVersion, (c) =>
                    new BetterAttribute(c).SetId(AttributeId.PACKAGE_PROVIDES_COMPATIBLE));
        }
    }
}
