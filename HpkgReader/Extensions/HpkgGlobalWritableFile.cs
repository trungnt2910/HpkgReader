using HpkgReader.Model;

namespace HpkgReader.Extensions
{
    public class HpkgGlobalWritableFile
    {
        private static readonly string[] _updateTypes = { "keep-old", "manual", "auto-merge" };
        public string Path { get; set; }
        public HpkgWritableFileUpdateType UpdateType { get; set; }
        public bool IsDirectory { get; set; }

        public override string ToString()
        {
            return $"{Path} {(IsDirectory ? "directory " : "")}{_updateTypes[(int)UpdateType]}";
        }

        internal BetterAttribute ToAttribute(AttributeId id = null)
        {
            id = id ?? AttributeId.PACKAGE_GLOBAL_WRITABLE_FILE;
            return new BetterAttribute(id, Path)
                .GuessTypeAndEncoding()
                .WithChildren(new[]
                {
                    new BetterAttribute(AttributeId.PACKAGE_WRITABLE_FILE_UPDATE_TYPE, (uint)UpdateType)
                        .GuessTypeAndEncoding(),
                    new BetterAttribute(AttributeId.PACKAGE_IS_WRITABLE_DIRECTORY, IsDirectory ? 1u : 0u)
                        .GuessTypeAndEncoding()
                });
        }
    }
}
