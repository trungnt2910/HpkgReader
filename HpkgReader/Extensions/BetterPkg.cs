using HpkgReader.Compat;
using HpkgReader.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;

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
        public BetterPkgUrl HomePageUrl { get; set; }
        public BetterPkgUrl SourceUrl { get; set; }

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
                    BetterVersion = new BetterPkgVersion();
                    BetterVersion.Major = attr.GetValue<string>(_attributesContext);
                    foreach (var childAttr in attr.GetChildAttributes())
                    {
                        if (childAttr.AttributeId == AttributeId.PACKAGE_VERSION_MINOR)
                        {
                            BetterVersion.Minor = childAttr.GetValue<string>(_attributesContext);
                        }
                        else if (childAttr.AttributeId == AttributeId.PACKAGE_VERSION_MICRO)
                        {
                            BetterVersion.Micro = childAttr.GetValue<string>(_attributesContext);
                        }
                        else if (childAttr.AttributeId == AttributeId.PACKAGE_VERSION_PRE_RELEASE)
                        {
                            BetterVersion.PreRelease = childAttr.GetValue<string>(_attributesContext);
                        }
                        else if (childAttr.AttributeId == AttributeId.PACKAGE_VERSION_REVISION)
                        {
                            BetterVersion.Revision = childAttr.GetValue<BigInteger?>(_attributesContext)?.IntValue();
                        }
                    }
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
                        HomePageUrl = new BetterPkgUrl(url);
                    }
                }
                else if (attr.AttributeId == AttributeId.PACKAGE_SOURCE_URL)
                {
                    var url = attr.GetValue<string>(_attributesContext);
                    if (!string.IsNullOrEmpty(url))
                    {
                        SourceUrl = new BetterPkgUrl(url);
                    }
                }
            }
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
