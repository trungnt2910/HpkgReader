/*
 * Copyright 2018, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

using System.Collections.Generic;
using System.Text;

namespace HpkgReader.Model
{
    public class Pkg
    {
        private readonly string name;
        private readonly PkgVersion version;
        private readonly PkgArchitecture? architecture;
        private readonly string vendor;
        private readonly List<string> copyrights;
        private readonly List<string> licenses;
        private readonly string summary;
        private readonly string description;
        private readonly PkgUrl homePageUrl;

        public Pkg(
                string name,
                PkgVersion version,
                PkgArchitecture? architecture,
                string vendor,
                List<string> copyrights,
                List<string> licenses,
                string summary,
                string description,
                PkgUrl homePageUrl)
        {
            this.name = name;
            this.version = version;
            this.architecture = architecture;
            this.vendor = vendor;
            this.copyrights = copyrights;
            this.licenses = licenses;
            this.summary = summary;
            this.description = description;
            this.homePageUrl = homePageUrl;
        }

        public string Description => description;

        public string Summary => summary;

        public string Vendor => vendor;

        public PkgVersion Version => version;

        public string Name => name;

        public PkgArchitecture? Architecture => architecture;

        public List<string> Copyrights
        {
            get
            {
                if (null == copyrights)
                {
                    return new List<string>();
                }
                return copyrights;
            }
        }

        public List<string> Licenses
        {
            get
            {
                if (null == licenses)
                {
                    return new List<string>();
                }
                return licenses;
            }
        }

        public PkgUrl HomePageUrl => homePageUrl;

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(null == name ? "???" : name);
            stringBuilder.Append(" : ");
            stringBuilder.Append(null == version ? "???" : version.ToString());
            stringBuilder.Append(" : ");
            stringBuilder.Append(null == architecture ? "???" : Architecture.ToString());
            return stringBuilder.ToString();
        }

    }

}
