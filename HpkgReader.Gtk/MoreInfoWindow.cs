using System;
using System.Collections;
using Gtk;
using HpkgReader.Extensions;
using UI = Gtk.Builder.ObjectAttribute;

namespace HpkgReader.Gtk
{
    class MoreInfoWindow: Window
    {
        [UI("Grid_PackageProperties")] private readonly Grid _grid_PackageProperties = null;

        private int _rowCount = 0;

        public MoreInfoWindow(BetterPkg pkg) : this(new Builder("MoreInfoWindow.glade"), pkg) { }

        private MoreInfoWindow(Builder builder, BetterPkg pkg) : base(builder.GetRawOwnedObject("MoreInfoWindow"))
        {
            builder.Autoconnect(this);

            Modal = true;

            _grid_PackageProperties.ColumnHomogeneous = false;
            _grid_PackageProperties.RowHomogeneous = false;

            
            SetupProperty(pkg.Provides, "Provides");
            SetupProperty(pkg.Requires, "Requires");
            SetupProperty(pkg.Supplements, "Supplements");
            SetupProperty(pkg.Conflicts, "Conflicts");
            SetupProperty(pkg.Freshens, "Freshens");
            SetupProperty(pkg.GlobalWritableFiles, "Global writable files");
            SetupProperty(pkg.UserSettingsFiles, "User settings files");
            SetupProperty(pkg.RequiredUsers, "Required users");
            SetupProperty(pkg.PostInstallScripts, "Post-install scripts");
            SetupProperty(pkg.PreUninstallScripts, "Pre-uninstall scripts");
        }

        private void SetupProperty(IList list, string labelText)
        {
            var index = _rowCount;
            ++_rowCount;

            _grid_PackageProperties.Attach(
                new Label()
                {
                    Text = $"{labelText}:",
                    Visible = true
                },
                0, index, 1, 1
                );


            var box = new ComboBoxText() { Visible = true };
            foreach (var item in list)
            {
                box.AppendText(item.ToString());
            }
            box.Active = 0;

            _grid_PackageProperties.Attach(
                box,
                1, index, 1, 1
                );

            _grid_PackageProperties.Attach(
                new Button()
                {
                    Label = "Edit",
                    Visible = true
                },
                2, index, 1, 1
                );
        }
    }
}
