using HpkgReader.Compat;

namespace HpkgReader.Extensions
{
    public class HpkgExtendedFileAttribute
    {
        public string Name { get; set; }
        public uint? Type { get; set; }
        public ByteSource Data { get; set; }

        public override string ToString()
        {
            return Name + ((Type != null) ? $" - Type: {Type}" : "");
        }
    }
}
