namespace HpkgReader.Extensions
{
    internal enum HpkgAttributeEncoding
    {
        INT_8_BIT = 0,
        INT_16_BIT = 1,
        INT_32_BIT = 2,
        INT_64_BIT = 3,

        STRING_INLINE = 0,
        STRING_TABLE = 1,

        RAW_INLINE = 0,
        RAW_HEAP = 1
    }
}
