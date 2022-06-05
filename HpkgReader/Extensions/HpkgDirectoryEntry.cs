using HpkgReader.Compat;
using HpkgReader.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Attribute = HpkgReader.Model.Attribute;

namespace HpkgReader.Extensions
{
    public class HpkgDirectoryEntry
    {
        public string Name { get; set; }
        public List<HpkgDirectoryEntry> Children { get; set; }
        public ByteSource Data { get; set; }
        public string Target { get; set; }

        public HpkgFileType FileType { get; set; } = HpkgFileType.FILE;
        public int? FilePermissions { get; set; }
        public string FileUser { get; set; }
        public string FileGroup { get; set; }
        public DateTime? FileAccessTime { get; set; }
        public DateTime? FileModifiedTime { get; set; }
        public DateTime? FileCreationTime { get; set; }
        public List<HpkgExtendedFileAttribute> FileAttributes { get; set; }

        public HpkgDirectoryEntry(Attribute attribute, AttributeContext context)
        {
            Debug.Assert(attribute.AttributeId == AttributeId.DIRECTORY_ENTRY, "Needs to be a directory entry.");

            Name = attribute.GetValue<string>(context);
            FileType = (HpkgFileType)(uint)(attribute.TryGetChildAttribute(AttributeId.FILE_TYPE)?.GetValue<BigInteger?>(context)?.IntValue() ?? 0);

            switch (FileType)
            {
                case HpkgFileType.FILE:
                    Data = attribute.TryGetChildAttribute(AttributeId.DATA)?.GetValue<ByteSource>(context);
                    break;
                case HpkgFileType.DIRECTORY:
                    Children = attribute.GetChildAttributes(AttributeId.DIRECTORY_ENTRY)
                        .Select(entry => new HpkgDirectoryEntry(entry, context))
                        .ToList();
                    break;
                case HpkgFileType.SYMLINK:
                    Target = attribute.TryGetChildAttribute(AttributeId.SYMLINK_PATH)?.GetValue<string>(context);
                    break;
            }

            FilePermissions = attribute.TryGetChildAttribute(AttributeId.FILE_PERMISSIONS)?.GetValue<BigInteger?>(context)?.IntValue();
            FileUser = attribute.TryGetChildAttribute(AttributeId.FILE_USER)?.GetValue<string>(context);
            FileGroup = attribute.TryGetChildAttribute(AttributeId.FILE_GROUP)?.GetValue<string>(context);
            FileAccessTime = GetTimeFromAttributes(attribute, AttributeId.FILE_ATIME, AttributeId.FILE_ATIME_NANOS, context);
            FileModifiedTime = GetTimeFromAttributes(attribute, AttributeId.FILE_MTIME, AttributeId.FILE_MTIME_NANOS, context);
            FileCreationTime = GetTimeFromAttributes(attribute, AttributeId.FILE_CRTIME, AttributeId.FILE_CRTIM_NANOS, context);
            FileAttributes = GetExtendedFileAttributesFromAttribute(attribute, context);
        }

        private DateTime? GetTimeFromAttributes(Attribute attribute, AttributeId attributeId, AttributeId attributeIdNanos, AttributeContext context)
        {
            long? time = attribute.TryGetChildAttribute(attributeId)?.GetValue<BigInteger?>(context)?.Multiply(1000).LongValue();
            if (time != null)
            {
                time += attribute.TryGetChildAttribute(attributeIdNanos)?.GetValue<BigInteger?>(context)?.Divide(1000).LongValue() ?? 0;
            }
            return (time != null) ? (DateTime?)DateTimeOffset.FromUnixTimeMilliseconds((long)time).UtcDateTime : null;
        }

        private List<HpkgExtendedFileAttribute> GetExtendedFileAttributesFromAttribute(Attribute attribute, AttributeContext context)
        {
            var result = new List<HpkgExtendedFileAttribute>();
            var attributes = attribute.GetChildAttributes(AttributeId.FILE_ATTRIBUTE);
            foreach (var fileAttribute in attributes)
            {
                var entry = new HpkgExtendedFileAttribute();
                entry.Name = fileAttribute.GetValue<string>(context);
                entry.Type = (uint?)fileAttribute.TryGetChildAttribute(AttributeId.FILE_ATTRIBUTE_TYPE).GetValue<BigInteger>(context);
                entry.Data = fileAttribute.TryGetChildAttribute(AttributeId.DATA).GetValue<ByteSource>(context);
                result.Add(entry);
            }
            return result;
        }
    }
}
