// headers/os/package/PackageFlags.h

using System;

namespace HpkgReader.Extensions
{
    [Flags]
    public enum PkgFlags : uint
    {
        NONE = 0,
        APPROVE_LICENSE = 1 << 0,
        SYSTEM_PACKAGE = 1 << 1
    }
}
