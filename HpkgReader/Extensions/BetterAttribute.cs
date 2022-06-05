using HpkgReader.Compat;
using HpkgReader.Heap;
using HpkgReader.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HpkgReader.Extensions
{
    /// <summary>
    /// A <see cref="Attribute"/> with mutable values.
    /// </summary>
    internal class BetterAttribute
    {
        public HpkgAttributeEncoding Encoding { get; set; }
        public bool HasChildren => (Children?.Count ?? 0) > 0;
        public List<BetterAttribute> Children { get; set; } = new List<BetterAttribute>();
        public HpkgAttributeType Type { get; set; }
        public AttributeId Id { get; set; }
        public object Value { get; set; }

        public BetterAttribute(AttributeId id, object value)
        {
            Id = id;
            Value = value;
        }

        public BetterAttribute(HpkgDirectoryEntry entry, Dictionary<ByteSource, HeapCoordinates> heapCoordinatesDictionary = null)
            : this(AttributeId.DIRECTORY_ENTRY, entry.Name)
        {
            GuessTypeAndEncoding();
            Children = new List<BetterAttribute>
            {
                new BetterAttribute(AttributeId.FILE_TYPE, (uint)entry.FileType).GuessTypeAndEncoding()
            };

            // This stuff must be added _before_ AttributeId.FILE_ATTRIBUTE, else subtle bugs might happen.
            // See haiku/src/kits/package/hpkg/PackageReaderImpl.cpp for more details.
            switch (entry.FileType)
            {
                case HpkgFileType.FILE:
                    if ((entry.Data != null) && (heapCoordinatesDictionary?.TryGetValue(entry.Data, out HeapCoordinates coords) ?? false))
                    {
                        AddChildIfNotNull(AttributeId.DATA, coords);
                    }
                    else
                    {
                        AddChildIfNotNull(AttributeId.DATA, entry.Data?.Read());
                    }
                break;
                case HpkgFileType.DIRECTORY:
                    Children.AddRange(entry.Children.Select(c => new BetterAttribute(c, heapCoordinatesDictionary)));
                break;
                case HpkgFileType.SYMLINK:
                    AddChildIfNotNull(AttributeId.SYMLINK_PATH, entry.Target);
                break;
            }

            AddChildIfNotNull(AttributeId.FILE_PERMISSIONS, (uint?)entry.FilePermissions);
            AddChildIfNotNull(AttributeId.FILE_USER, entry.FileUser);
            AddChildIfNotNull(AttributeId.FILE_GROUP, entry.FileGroup);
            AddChildIfNotNull(AttributeId.FILE_ATIME, (uint?)(((DateTimeOffset?)entry.FileAccessTime)?.ToUnixTimeSeconds()));
            AddChildIfNotNull(AttributeId.FILE_ATIME_NANOS, (uint?)(((DateTimeOffset?)entry.FileAccessTime)?.ToUnixTimeMilliseconds() % 1000 * 1000));
            AddChildIfNotNull(AttributeId.FILE_MTIME, (uint?)(((DateTimeOffset?)entry.FileModifiedTime)?.ToUnixTimeSeconds()));
            AddChildIfNotNull(AttributeId.FILE_MTIME_NANOS, (uint?)(((DateTimeOffset?)entry.FileModifiedTime)?.ToUnixTimeMilliseconds() % 1000 * 1000));
            AddChildIfNotNull(AttributeId.FILE_CRTIME, (uint?)(((DateTimeOffset?)entry.FileCreationTime)?.ToUnixTimeSeconds()));
            AddChildIfNotNull(AttributeId.FILE_CRTIM_NANOS, (uint?)(((DateTimeOffset?)entry.FileCreationTime)?.ToUnixTimeMilliseconds() % 1000 * 1000));

            // Add this last.
            foreach (var fa in entry.FileAttributes)
            {
                var faAttribute = new BetterAttribute(AttributeId.FILE_ATTRIBUTE, fa.Name)
                    .GuessTypeAndEncoding()
                    .WithChildIfNotNull(fa.Type, f => new BetterAttribute(AttributeId.FILE_ATTRIBUTE_TYPE, f).GuessTypeAndEncoding())
                    .WithChildIfNotNull(fa.Data, d =>
                     {
                         if (heapCoordinatesDictionary?.TryGetValue(d, out HeapCoordinates coords) ?? false)
                         {
                             return new BetterAttribute(AttributeId.DATA, coords).GuessTypeAndEncoding();
                         }
                         else
                         {
                             return new BetterAttribute(AttributeId.DATA, d.Read()).GuessTypeAndEncoding();
                         }
                     });
                Children.Add(faAttribute);
            }
        }

        public BetterAttribute(BetterPkgVersion version)
            : this(AttributeId.PACKAGE_VERSION_MAJOR, version.Major)
        {
            GuessTypeAndEncoding();
            Children = new List<BetterAttribute>();
            AddChildIfNotNull(AttributeId.PACKAGE_VERSION_MINOR, version.Minor);
            AddChildIfNotNull(AttributeId.PACKAGE_VERSION_MICRO, version.Micro);
            AddChildIfNotNull(AttributeId.PACKAGE_VERSION_PRE_RELEASE, version.PreRelease);
            AddChildIfNotNull(AttributeId.PACKAGE_VERSION_REVISION, version.Revision);
        }

        private void AddChildIfNotNull(AttributeId id, object value)
        {
            if (value != null)
            {
                Children.Add(new BetterAttribute(id, value).GuessTypeAndEncoding());
            }
        }

        public BetterAttribute GuessTypeAndEncoding()
        {
            switch (Value)
            {
                case byte _:
                case ushort _:
                case uint _:
                case ulong _:
                    Type = HpkgAttributeType.UINT;
                break;
                case sbyte _:
                case short _:
                case int _:
                case long _:
                    Type = HpkgAttributeType.INT;
                break;
                case string _:
                    Type = HpkgAttributeType.STRING;
                break;
                case byte[] _:
                case HeapCoordinates _:
                    Type = HpkgAttributeType.RAW;
                break;
            }

            switch (Value)
            {
                case byte _:
                case sbyte _:
                    Encoding = HpkgAttributeEncoding.INT_8_BIT;
                break;
                case ushort _:
                case short _:
                    Encoding = HpkgAttributeEncoding.INT_16_BIT;
                break;
                case uint _:
                case int _:
                    Encoding = HpkgAttributeEncoding.INT_32_BIT;
                break;
                case ulong _:
                case long _:
                    Encoding = HpkgAttributeEncoding.INT_64_BIT;
                break;
                case string _:
                    Encoding = HpkgAttributeEncoding.STRING_INLINE;
                break;
                case byte[] _:
                    Encoding = HpkgAttributeEncoding.RAW_INLINE;
                break;
                case HeapCoordinates _:
                    Encoding = HpkgAttributeEncoding.RAW_HEAP;
                break;
            }
            return this;
        }

        public BetterAttribute WithChildren(IEnumerable<BetterAttribute> children)
        {
            Children.AddRange(children);
            return this;
        }

        public BetterAttribute WithChild(BetterAttribute child)
        {
            Children.Add(child);
            return this;
        }

        public BetterAttribute WithChildIfNotNull<T>(T obj, Func<T, BetterAttribute> childCreator)
        {
            if (obj != null)
            {
                Children.Add(childCreator(obj));
            }
            return this;
        }

        public BetterAttribute SetId(AttributeId id)
        {
            Id = id;
            return this;
        }
    }
}
