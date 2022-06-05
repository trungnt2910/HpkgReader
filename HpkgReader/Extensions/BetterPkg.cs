using HpkgReader.Compat;
using HpkgReader.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Attribute = HpkgReader.Model.Attribute;

namespace HpkgReader.Extensions
{
    /// <summary>
    /// A class, only available in the .NET version, that offers a better package 
    /// model than <see cref="Model.Pkg"/>
    /// </summary>
    public class BetterPkg : IDisposable
    {
        private readonly HpkgFileExtractor _extractor;
        private readonly AttributeContext _attributesContext;
        private readonly AttributeContext _tocContext;
        private bool _disposedValue;

        public string Name { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public string Vendor { get; set; }
        public string Packager { get; set; } = Assembly.GetExecutingAssembly().GetName().Name;
        public string BasePackage { get; set; }
        public PkgFlags Flags { get; set; }
        public PkgArchitecture? Architecture { get; set; }
        public BetterPkgVersion BetterVersion { get; set; }
        [Obsolete("This member is provided for compatibility with Pkg only. Use BetterVersion instead.")]
        public PkgVersion Version
        {
            get => (PkgVersion)BetterVersion;
            set
            {
                BetterVersion = new BetterPkgVersion()
                {
                    Major = value.Major,
                    Minor = value.Minor,
                    Micro = value.Micro,
                    PreRelease = value.PreRelease,
                    Revision = value.Revision
                };
            }
        }
        public List<string> Copyrights { get; set; } = new List<string>();
        public List<string> Licenses { get; set; } = new List<string>();
        public List<BetterPkgUrl> HomePageUrls { get; set; } = new List<BetterPkgUrl>();
        public List<BetterPkgUrl> SourceUrls { get; set; } = new List<BetterPkgUrl>();
        public List<HpkgCompatibleEntity> Provides { get; set; } = new List<HpkgCompatibleEntity>();
        public List<HpkgResolvableEntity> Requires { get; set; } = new List<HpkgResolvableEntity>();
        public List<HpkgResolvableEntity> Supplements { get; set; } = new List<HpkgResolvableEntity>();
        public List<HpkgResolvableEntity> Conflicts { get; set; } = new List<HpkgResolvableEntity>();
        public List<HpkgResolvableEntity> Freshens { get; set; } = new List<HpkgResolvableEntity>();
        public List<string> Replaces { get; set; } = new List<string>();
        public List<HpkgGlobalWritableFile> GlobalWritableFiles { get; set; } = new List<HpkgGlobalWritableFile>();
        public List<HpkgUserSettingsFile> UserSettingsFiles { get; set; } = new List<HpkgUserSettingsFile>();
        public List<HpkgPackageUser> RequiredUsers { get; set; } = new List<HpkgPackageUser>();
        public List<string> RequiredGroups { get; set; } = new List<string>();
        public List<string> PostInstallScripts { get; set; } = new List<string>();
        public List<string> PreUninstallScripts { get; set; } = new List<string>();

        public List<HpkgDirectoryEntry> DirectoryEntries { get; set; }

        /// <summary>
        /// Creates an empty <see cref="BetterPkg"/>
        /// </summary>
        public BetterPkg()
        {
            _extractor = null;
            _attributesContext = null;
            _tocContext = null;
        }

        /// <summary>
        /// Creates a <see cref="BetterPkg"/> from an existing Hpkg file.
        /// </summary>
        /// <param name="filePath">An existing Hpkg file.</param>
        public BetterPkg(string filePath)
        {
            _extractor = new HpkgFileExtractor(new FileInfo(filePath));
            _attributesContext = _extractor.GetPackageAttributeContext();
            _tocContext = _extractor.GetTocContext();

            var packageAttributes = _extractor.GetPackageAttributes();
            var toc = _extractor.GetToc();

            DirectoryEntries = toc
                .Where(a => a.AttributeId == AttributeId.DIRECTORY_ENTRY)
                .Select(a => new HpkgDirectoryEntry(a, _tocContext))
                .ToList();

            foreach (var attr in packageAttributes)
            {
                if (attr.AttributeId == AttributeId.PACKAGE_NAME)
                {
                    Name = attr.GetValue<string>(_attributesContext);
                }
                else if (attr.AttributeId == AttributeId.PACKAGE_SUMMARY)
                {
                    Summary = attr.GetValue<string>(_attributesContext);
                }
                else if (attr.AttributeId == AttributeId.PACKAGE_DESCRIPTION)
                {
                    Description = attr.GetValue<string>(_attributesContext);
                }
                else if (attr.AttributeId == AttributeId.PACKAGE_VENDOR)
                {
                    Vendor = attr.GetValue<string>(_attributesContext);
                }
                else if (attr.AttributeId == AttributeId.PACKAGE_PACKAGER)
                {
                    Packager = attr.GetValue<string>(_attributesContext);
                }
                else if (attr.AttributeId == AttributeId.PACKAGE_BASE_PACKAGE)
                {
                    BasePackage = attr.GetValue<string>(_attributesContext);
                }
                else if (attr.AttributeId == AttributeId.PACKAGE_FLAGS)
                {
                    var intFlags = attr.GetValue<BigInteger?>(_attributesContext)?.IntValue();
                    if (intFlags != null)
                    {
                        Flags = (PkgFlags)(uint)(int)intFlags;
                    }
                }
                else if (attr.AttributeId == AttributeId.PACKAGE_ARCHITECTURE)
                {
                    var archInt = attr.GetValue<BigInteger?>(_attributesContext)?.IntValue();
                    Architecture = (archInt == null) ? (PkgArchitecture?)null : (PkgArchitecture)(int)archInt;
                }
                else if (attr.AttributeId == AttributeId.PACKAGE_VERSION_MAJOR)
                {
                    BetterVersion = ReadVersionFromAttribute(attr, _attributesContext);
                }
                else if (attr.AttributeId == AttributeId.PACKAGE_COPYRIGHT)
                {
                    var copyright = attr.GetValue<string>(_attributesContext);
                    if (copyright != null)
                    {
                        Copyrights.Add(copyright);
                    }
                }
                else if (attr.AttributeId == AttributeId.PACKAGE_LICENSE)
                {
                    var license = attr.GetValue<string>(_attributesContext);
                    if (license != null)
                    {
                        Licenses.Add(license);
                    }
                }
                else if (attr.AttributeId == AttributeId.PACKAGE_URL)
                {
                    var url = attr.GetValue<string>(_attributesContext);
                    if (!string.IsNullOrEmpty(url))
                    {
                        HomePageUrls.Add(new BetterPkgUrl(url));
                    }
                }
                else if (attr.AttributeId == AttributeId.PACKAGE_SOURCE_URL)
                {
                    var url = attr.GetValue<string>(_attributesContext);
                    if (!string.IsNullOrEmpty(url))
                    {
                        SourceUrls.Add(new BetterPkgUrl(url));
                    }
                }
                else if (attr.AttributeId == AttributeId.PACKAGE_PROVIDES)
                {
                    Provides.Add(ReadCompatibleEntityFromAttribute(attr, _attributesContext));
                }
                else if (attr.AttributeId == AttributeId.PACKAGE_REQUIRES)
                {
                    Requires.Add(ReadResolvableEntityFromAttribute(attr, _attributesContext));
                }
                else if (attr.AttributeId == AttributeId.PACKAGE_SUPPLEMENTS)
                {
                    Supplements.Add(ReadResolvableEntityFromAttribute(attr, _attributesContext));
                }
                else if (attr.AttributeId == AttributeId.PACKAGE_CONFLICTS)
                {
                    Conflicts.Add(ReadResolvableEntityFromAttribute(attr, _attributesContext));
                }
                else if (attr.AttributeId == AttributeId.PACKAGE_FRESHENS)
                {
                    Freshens.Add(ReadResolvableEntityFromAttribute(attr, _attributesContext));
                }
                else if (attr.AttributeId == AttributeId.PACKAGE_REPLACES)
                {
                    var replace = attr.GetValue<string>(_attributesContext);
                    if (replace != null)
                    {
                        Replaces.Add(replace);
                    }
                }
                else if (attr.AttributeId == AttributeId.PACKAGE_CHECKSUM)
                {
                    throw new InvalidOperationException("How could you take your own checksum????");
                }
                else if (attr.AttributeId == AttributeId.PACKAGE_GLOBAL_WRITABLE_FILE)
                {
                    GlobalWritableFiles.Add(ReadGlobalWritableFileFromAttribute(attr, _attributesContext));
                }
                else if (attr.AttributeId == AttributeId.PACKAGE_USER_SETTINGS_FILE)
                {
                    UserSettingsFiles.Add(ReadUserSettingsFileFromAttribute(attr, _attributesContext));
                }
                else if (attr.AttributeId == AttributeId.PACKAGE_USER)
                {
                    RequiredUsers.Add(ReadUserFromAttribute(attr, _attributesContext));
                }
                else if (attr.AttributeId == AttributeId.PACKAGE_GROUP)
                {
                    RequiredGroups.Add(attr.GetValue<string>(_attributesContext));
                }
                else if (attr.AttributeId == AttributeId.PACKAGE_POST_INSTALL_SCRIPT)
                {
                    PostInstallScripts.Add(attr.GetValue<string>(_attributesContext));
                }
                else if (attr.AttributeId == AttributeId.PACKAGE_PRE_UNINSTALL_SCRIPT)
                {
                    PreUninstallScripts.Add(attr.GetValue<string>(_attributesContext));
                }
            }
        }

        private static BetterPkgVersion ReadVersionFromAttribute(Attribute attr, AttributeContext context)
        {
            var version = new BetterPkgVersion();
            version.Major = attr.GetValue<string>(context);
            foreach (var childAttr in attr.GetChildAttributes())
            {
                if (childAttr.AttributeId == AttributeId.PACKAGE_VERSION_MINOR)
                {
                    version.Minor = childAttr.GetValue<string>(context);
                }
                else if (childAttr.AttributeId == AttributeId.PACKAGE_VERSION_MICRO)
                {
                    version.Micro = childAttr.GetValue<string>(context);
                }
                else if (childAttr.AttributeId == AttributeId.PACKAGE_VERSION_PRE_RELEASE)
                {
                    version.PreRelease = childAttr.GetValue<string>(context);
                }
                else if (childAttr.AttributeId == AttributeId.PACKAGE_VERSION_REVISION)
                {
                    version.Revision = childAttr.GetValue<BigInteger?>(context)?.IntValue();
                }
            }
            return version;
        }

        private static HpkgCompatibleEntity ReadCompatibleEntityFromAttribute(Attribute attr, AttributeContext context)
        {
            var entity = new HpkgCompatibleEntity();
            entity.Name = attr.GetValue<string>(context);
            foreach (var childAttr in attr.GetChildAttributes())
            {
                if (childAttr.AttributeId == AttributeId.PACKAGE_VERSION_MAJOR)
                {
                    entity.Version = ReadVersionFromAttribute(childAttr, context);
                }
                else if (childAttr.AttributeId == AttributeId.PACKAGE_PROVIDES_COMPATIBLE)
                {
                    entity.CompatibleVersion = ReadVersionFromAttribute(childAttr, context);
                }
            }
            return entity;
        }

        private static HpkgResolvableEntity ReadResolvableEntityFromAttribute(Attribute attr, AttributeContext context)
        {
            var entity = new HpkgResolvableEntity();
            entity.Name = attr.GetValue<string>(context);
            foreach (var childAttr in attr.GetChildAttributes())
            {
                if (childAttr.AttributeId == AttributeId.PACKAGE_RESOLVABLE_OPERATOR)
                {
                    entity.ResolvableOperator = (HpkgResolvableOperator)(int)childAttr.GetValue<BigInteger>(context);
                }
                else if (childAttr.AttributeId == AttributeId.PACKAGE_VERSION_MAJOR)
                {
                    entity.Version = ReadVersionFromAttribute(childAttr, context);
                }
            }
            return entity;
        }

        private static HpkgGlobalWritableFile ReadGlobalWritableFileFromAttribute(Attribute attr, AttributeContext context)
        {
            var file = new HpkgGlobalWritableFile();
            file.Path = attr.GetValue<string>(context);
            file.UpdateType = (HpkgWritableFileUpdateType)
                (attr.TryGetChildAttribute(AttributeId.PACKAGE_WRITABLE_FILE_UPDATE_TYPE)?.GetValue<BigInteger>(context).IntValue() ?? 0);
            file.IsDirectory = attr.TryGetChildAttribute(AttributeId.PACKAGE_IS_WRITABLE_DIRECTORY)?.GetValue<BigInteger>(context).IntValue() == 1;
            return file;
        }

        private static HpkgUserSettingsFile ReadUserSettingsFileFromAttribute(Attribute attr, AttributeContext context)
        {
            var file = new HpkgUserSettingsFile();
            file.Path = attr.GetValue<string>(context);
            file.TemplatePath = attr.TryGetChildAttribute(AttributeId.PACKAGE_SETTINGS_FILE_TEMPLATE)?.GetValue<string>(context);
            file.IsDirectory = attr.TryGetChildAttribute(AttributeId.PACKAGE_IS_WRITABLE_DIRECTORY)?.GetValue<BigInteger>(context).IntValue() == 1;
            return file;
        }

        private static HpkgPackageUser ReadUserFromAttribute(Attribute attr, AttributeContext context)
        {
            var user = new HpkgPackageUser();
            user.Name = attr.GetValue<string>(context);
            foreach (var childAttr in attr.GetChildAttributes())
            {
                if (childAttr.AttributeId == AttributeId.PACKAGE_USER_REAL_NAME)
                {
                    user.RealName = childAttr.GetValue<string>(context);
                }
                else if (childAttr.AttributeId == AttributeId.PACKAGE_USER_HOME)
                {
                    user.Home = childAttr.GetValue<string>(context);
                }
                else if (childAttr.AttributeId == AttributeId.PACKAGE_USER_SHELL)
                {
                    user.Shell = childAttr.GetValue<string>(context);
                }
                else if (childAttr.AttributeId == AttributeId.PACKAGE_USER_GROUP)
                {
                    user.Groups.Add(childAttr.GetValue<string>(context));
                }
            }
            return user;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _extractor?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~BetterPkg()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
