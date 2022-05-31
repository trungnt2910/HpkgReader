using Gtk;
using HpkgReader.Compat;
using HpkgReader.Extensions;
using HpkgReader.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Attribute = HpkgReader.Model.Attribute;
using UI = Gtk.Builder.ObjectAttribute;

namespace HpkgReader.Gtk
{
    internal class MainWindow : Window
    {
        [UI("VBox_Root")] private VBox _vBox_Root = null;
        [UI("ImageMenuItem_Open")] private ImageMenuItem _imageMenuItem_Open = null;
        [UI("Entry_Name")] private Entry _entry_Name = null;
        [UI("Entry_Version")] private Entry _entry_Version = null;
        [UI("Entry_Arch")] private Entry _entry_Arch = null;
        [UI("Entry_Vendor")] private Entry _entry_Vendor = null;
        [UI("Entry_Summary")] private Entry _entry_Summary = null;
        [UI("ComboBoxText_Copyrights")] private ComboBoxText _comboBox_Copyrights = null;
        [UI("ComboBoxText_Licenses")] private ComboBoxText _comboBox_Licenses = null;
        [UI("TextView_Description")] private TextView _textView_Description = null;
        [UI("Link_HomePage")] private LinkButton _link_HomePage = null;

        private readonly TreeView _treeView_PackageContents = new TreeView();
        private readonly TreeStore _treeStore_Files = new TreeStore(typeof(string), typeof(string));

        public MainWindow() : this(new Builder("MainWindow.glade")) { }

        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);

            DeleteEvent += Window_DeleteEvent;
            _imageMenuItem_Open.Activated += ImageMenuItem_Open_Clicked;

            TreeViewColumn CreateTreeViewTextColumn(string title, int colPos)
            {
                var col = new TreeViewColumn()
                {
                    Title = title
                };

                var renderer = new CellRendererText();
                col.PackStart(renderer, true);
                col.AddAttribute(renderer, "text", colPos);

                return col;
            }

            _treeView_PackageContents.AppendColumn(
                CreateTreeViewTextColumn("Name", 0)
                );
            _treeView_PackageContents.AppendColumn(
                CreateTreeViewTextColumn("Size", 1)
                );

            _treeView_PackageContents.Model = _treeStore_Files;

            _vBox_Root.Add(_treeView_PackageContents);
            _vBox_Root.PackEnd(_treeView_PackageContents, true, true, 0);
            _treeView_PackageContents.Show();
        }

        private void ImageMenuItem_Open_Clicked(object o, EventArgs args)
        {
            using var dialog = new FileChooserDialog("Open a HPKG file", this, FileChooserAction.Open, 
                "Cancel", ResponseType.Cancel, 
                "Open", ResponseType.Accept,
                null);
            if (dialog.Run() == (int)ResponseType.Accept)
            {
                var hpkgFile = dialog.Filename;
                using var reader = new HpkgFileExtractor(new FileInfo(hpkgFile));

                var pkg = reader.CreatePkg();

                _entry_Name.Text = pkg.Name;
                _entry_Version.Text = pkg.Version.ToString();
                _entry_Arch.Text = pkg.Architecture.ToString();
                _entry_Vendor.Text = pkg.Vendor;
                _entry_Summary.Text = pkg.Summary;

                _comboBox_Copyrights.RemoveAll();
                foreach (var copyright in pkg.Copyrights)
                {
                    _comboBox_Copyrights.AppendText(copyright);
                }
                // Display the most recent copyright holder.
                _comboBox_Copyrights.Active = pkg.Copyrights.Count - 1;

                _comboBox_Licenses.RemoveAll();
                foreach (var license in pkg.Licenses)
                {
                    _comboBox_Licenses.AppendText(license);
                }
                _comboBox_Licenses.Active = 0;

                _textView_Description.Buffer.Text = pkg.Description;
                _link_HomePage.Uri = pkg.HomePageUrl.Url;
                _link_HomePage.Label = string.IsNullOrEmpty(pkg.HomePageUrl.Name) ? pkg.HomePageUrl.Name : pkg.HomePageUrl.Url;

                _treeStore_Files.Clear();
                ReadHpkgFiles(reader);
            }
            dialog.Destroy();
        }

        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }

        private void ReadHpkgFiles(HpkgFileExtractor reader)
        {
            var toc = reader.GetToc();
            var context = reader.GetTocContext();

            void IterateToc(TreeIter iter, List<Attribute> attributes, AttributeContext context)
            {
                foreach (var attr in attributes)
                {
                    if (attr.AttributeId == AttributeId.DIRECTORY_ENTRY)
                    {
                        var data = attr.TryGetChildAttribute(AttributeId.DATA);
                        var size = data?.GetValue<ByteSource>(context).SizeIfKnown()?.ToString() ?? "";
                        var childIter = _treeStore_Files.AppendValues(iter, attr.GetValue<string>(context), size);
                        IterateToc(childIter, attr.GetChildAttributes(), context);
                    }
                }
            }

            foreach (var attr in toc)
            {
                var iter = _treeStore_Files.AppendValues(attr.GetValue<string>(context), "0");
                IterateToc(iter, attr.GetChildAttributes(), context);
            }
        }
    }
}
