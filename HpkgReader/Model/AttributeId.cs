/*
 * Copyright 2018, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HpkgReader.Model
{
    /// <summary>
    /// <para>These constants define the meaning of an <see cref="Attribute"/>.  The numerical value is a value that comes up
    /// in the formatted file that maps to these constants. The string value is a name for the attribute and the type 
    /// gives the type that is expected to be associated with an <see cref="Attribute"/> that has one of these IDs.</para> 
    /// 
    /// <para>These constants were obtained from 
    /// <a href="http://cgit.haiku-os.org/haiku/tree/headers/os/package/hpkg/PackageAttributes.h">here</a> and then the
    /// search/replace of <c>B_DEFINE_HPKG_ATTRIBUTE\([ ]*(\d+),[ ]*([A-Z]+),[ \t]*("[a-z:\-.]+"),[ \t]*([A-Z_]+)\)</c>
    /// <c>$4($1,$3,$2),</c> was applied.</para>
    /// 
    /// <para>For the C# port, the codes were obtained from the Java version using the search/replace of
    /// <c>([A-Z_]*)\((.*)\)[,;]</c> <c>public static readonly AttributeId $1 = new AttributeId($2);</c></para>
    /// </summary>

    public class AttributeId
    {
        private static readonly List<AttributeId> _values = new List<AttributeId>();
        private static AttributeId[] _valuesArr;

        public static readonly AttributeId DIRECTORY_ENTRY = new AttributeId(0, "dir:entry", AttributeType.STRING);
        public static readonly AttributeId FILE_TYPE = new AttributeId(1, "file:type", AttributeType.INT);
        public static readonly AttributeId FILE_PERMISSIONS = new AttributeId(2, "file:permissions", AttributeType.INT);
        public static readonly AttributeId FILE_USER = new AttributeId(3, "file:user", AttributeType.STRING);
        public static readonly AttributeId FILE_GROUP = new AttributeId(4, "file:group", AttributeType.STRING);
        public static readonly AttributeId FILE_ATIME = new AttributeId(5, "file:atime", AttributeType.INT);
        public static readonly AttributeId FILE_MTIME = new AttributeId(6, "file:mtime", AttributeType.INT);
        public static readonly AttributeId FILE_CRTIME = new AttributeId(7, "file:crtime", AttributeType.INT);
        public static readonly AttributeId FILE_ATIME_NANOS = new AttributeId(8, "file:atime:nanos", AttributeType.INT);
        public static readonly AttributeId FILE_MTIME_NANOS = new AttributeId(9, "file:mtime:nanos", AttributeType.INT);
        public static readonly AttributeId FILE_CRTIM_NANOS = new AttributeId(10, "file:crtime:nanos", AttributeType.INT);
        public static readonly AttributeId FILE_ATTRIBUTE = new AttributeId(11, "file:attribute", AttributeType.STRING);
        public static readonly AttributeId FILE_ATTRIBUTE_TYPE = new AttributeId(12, "file:attribute:type", AttributeType.INT);
        public static readonly AttributeId DATA = new AttributeId(13, "data", AttributeType.RAW);
        public static readonly AttributeId SYMLINK_PATH = new AttributeId(14, "symlink:path", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_NAME = new AttributeId(15, "package:name", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_SUMMARY = new AttributeId(16, "package:summary", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_DESCRIPTION = new AttributeId(17, "package:description", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_VENDOR = new AttributeId(18, "package:vendor", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_PACKAGER = new AttributeId(19, "package:packager", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_FLAGS = new AttributeId(20, "package:flags", AttributeType.INT);
        public static readonly AttributeId PACKAGE_ARCHITECTURE = new AttributeId(21, "package:architecture", AttributeType.INT);
        public static readonly AttributeId PACKAGE_VERSION_MAJOR = new AttributeId(22, "package:version.major", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_VERSION_MINOR = new AttributeId(23, "package:version.minor", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_VERSION_MICRO = new AttributeId(24, "package:version.micro", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_VERSION_REVISION = new AttributeId(25, "package:version.revision", AttributeType.INT);
        public static readonly AttributeId PACKAGE_COPYRIGHT = new AttributeId(26, "package:copyright", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_LICENSE = new AttributeId(27, "package:license", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_PROVIDES = new AttributeId(28, "package:provides", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_REQUIRES = new AttributeId(29, "package:requires", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_SUPPLEMENTS = new AttributeId(30, "package:supplements", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_CONFLICTS = new AttributeId(31, "package:conflicts", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_FRESHENS = new AttributeId(32, "package:freshens", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_REPLACES = new AttributeId(33, "package:replaces", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_RESOLVABLE_OPERATOR = new AttributeId(34, "package:resolvable.operator", AttributeType.INT);
        public static readonly AttributeId PACKAGE_CHECKSUM = new AttributeId(35, "package:checksum", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_VERSION_PRE_RELEASE = new AttributeId(36, "package:version.prerelease", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_PROVIDES_COMPATIBLE = new AttributeId(37, "package:provides.compatible", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_URL = new AttributeId(38, "package:url", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_SOURCE_URL = new AttributeId(39, "package:source-url", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_INSTALL_PATH = new AttributeId(40, "package:install-path", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_BASE_PACKAGE = new AttributeId(41, "package:base-package", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_GLOBAL_WRITABLE_FILE = new AttributeId(42, "package:global-writable-file", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_USER_SETTINGS_FILE = new AttributeId(43, "package:user-settings-file", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_WRITABLE_FILE_UPDATE_TYPE = new AttributeId(44, "package:writable-file-update-type", AttributeType.INT);
        public static readonly AttributeId PACKAGE_SETTINGS_FILE_TEMPLATE = new AttributeId(45, "package:settings-file-template", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_USER = new AttributeId(46, "package:user", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_USER_REAL_NAME = new AttributeId(47, "package:user.real-name", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_USER_HOME = new AttributeId(48, "package:user.home", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_USER_SHELL = new AttributeId(49, "package:user.shell", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_USER_GROUP = new AttributeId(50, "package:user.group", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_GROUP = new AttributeId(51, "package:group", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_POST_INSTALL_SCRIPT = new AttributeId(52, "package:post-install-script", AttributeType.STRING);
        public static readonly AttributeId PACKAGE_IS_WRITABLE_DIRECTORY = new AttributeId(53, "package:is-writable-directory", AttributeType.INT);
        public static readonly AttributeId PACKAGE = new AttributeId(54, "package", AttributeType.STRING);

        private readonly int code;
        private readonly string name;
        private readonly AttributeType attributeType;

        AttributeId(int code, string name, AttributeType attributeType)
        {
            this.code = code;
            this.name = name;
            this.attributeType = attributeType;
            _values.Add(this);
            _valuesArr = null;
        }

        public int Code => code;

        public string Name => name;

        public AttributeType AttributeType => attributeType;

        /// <summary>
        /// Compatibility property for Java's Enum.values()
        /// </summary>
        public static AttributeId[] Values => _valuesArr ?? (_valuesArr = _values.ToArray());
    }
}