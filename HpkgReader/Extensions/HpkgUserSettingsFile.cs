using HpkgReader.Model;

namespace HpkgReader.Extensions
{
    public class HpkgUserSettingsFile
    {
        public string Path { get; set; }
        public string TemplatePath { get; set; }
        public bool IsDirectory { get; set; }

        public override string ToString()
        {
            return $"{Path}{(IsDirectory ? " directory" : "")}{((TemplatePath != null) ? $" template {TemplatePath}" : "")}";
        }

        internal BetterAttribute ToAttribute(AttributeId id = null)
        {
            id = id ?? AttributeId.PACKAGE_USER_SETTINGS_FILE;
            return new BetterAttribute(id, Path)
                .GuessTypeAndEncoding()
                .WithChildIfNotNull(TemplatePath, (p) =>
                    new BetterAttribute(AttributeId.PACKAGE_SETTINGS_FILE_TEMPLATE, p)
                        .GuessTypeAndEncoding())
                .WithChild(new BetterAttribute(AttributeId.PACKAGE_IS_WRITABLE_DIRECTORY, IsDirectory ? 1u : 0u)
                        .GuessTypeAndEncoding());
        }
    }
}
