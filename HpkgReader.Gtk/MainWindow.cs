using Gtk;
using HpkgReader.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using UI = Gtk.Builder.ObjectAttribute;

namespace HpkgReader.Gtk
{
    internal class MainWindow : Window
    {
        [UI("ImageMenuItem_Open")] private readonly ImageMenuItem _imageMenuItem_Open = null;
        [UI("ImageMenuItem_SaveAs")] private readonly ImageMenuItem _imageMenuItem_SaveAs = null;
        [UI("Entry_Name")] private readonly Entry _entry_Name = null;
        [UI("Entry_Version")] private readonly Entry _entry_Version = null;
        [UI("Entry_Arch")] private readonly Entry _entry_Arch = null;
        [UI("Entry_Vendor")] private readonly Entry _entry_Vendor = null;
        [UI("Entry_Packager")] private readonly Entry _entry_Packager = null;
        [UI("Entry_BasePackage")] private readonly Entry _entry_BasePackage = null;
        [UI("Entry_Flags")] private readonly Entry _entry_Flags = null;
        [UI("Entry_Summary")] private readonly Entry _entry_Summary = null;
        [UI("ComboBoxText_Copyrights")] private readonly ComboBoxText _comboBox_Copyrights = null;
        [UI("ComboBoxText_Licenses")] private readonly ComboBoxText _comboBox_Licenses = null;
        [UI("TextView_Description")] private readonly TextView _textView_Description = null;
        [UI("ComboBoxText_HomePageUrls")] private readonly ComboBoxText _comboBox_HomePageUrls = null;
        [UI("ComboBoxText_SourceUrls")] private readonly ComboBoxText _comboBox_SourceUrls = null;
        [UI("TreeView_PackageContents")] private readonly TreeView _treeView_PackageContents = null;
        [UI("Button_FilePreview")] private readonly Button _button_FilePreview = null;
        [UI("Button_FileExport")] private readonly Button _button_FileExport = null;
        [UI("Button_MoreProperties")] private readonly Button _button_MoreProperties = null;

        private readonly TreeStore _treeStore_Files = new TreeStore(typeof(HpkgDirectoryEntry));
        private BetterPkg _currentPkg;

        public MainWindow() : this(new Builder("MainWindow.glade")) { }

        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);

            DeleteEvent += Window_DeleteEvent;
            _imageMenuItem_Open.Activated += ImageMenuItem_Open_Clicked;
            _imageMenuItem_SaveAs.Activated += ImageMenuItem_SaveAs_Clicked;
            _button_FilePreview.Clicked += Button_FilePreview_Clicked;
            _button_MoreProperties.Clicked += Button_MoreProperties_Clicked;

            TreeViewColumn CreateTreeViewTextColumn(string title, int colPos, TreeCellDataFunc func)
            {
                var col = new TreeViewColumn()
                {
                    Title = title
                };

                var renderer = new CellRendererText();
                col.PackStart(renderer, true);
                col.AddAttribute(renderer, "text", colPos);
                col.SetCellDataFunc(renderer, func);

                return col;
            }

            _treeView_PackageContents.AppendColumn(
                CreateTreeViewTextColumn("Name", 0,
                (column, cell, model, iter) =>
                {
                    var entry = (HpkgDirectoryEntry)model.GetValue(iter, 0);
                    (cell as CellRendererText).Text = entry.Name;
                }));
            _treeView_PackageContents.AppendColumn(
                CreateTreeViewTextColumn("Size", 1,
                (column, cell, model, iter) =>
                {
                    var entry = (HpkgDirectoryEntry)model.GetValue(iter, 0);
                    if (entry.FileType == HpkgFileType.FILE)
                    {
                        (cell as CellRendererText).Text = entry.Data?.SizeIfKnown()?.ToString() ?? "????";
                    }
                    else
                    {
                        (cell as CellRendererText).Text = "";
                    }
                }));
            _treeView_PackageContents.AppendColumn(
                CreateTreeViewTextColumn("Type", 2,
                (column, cell, model, iter) =>
                {
                    var entry = (HpkgDirectoryEntry)model.GetValue(iter, 0);
                    (cell as CellRendererText).Text = entry.FileType switch
                    {
                        HpkgFileType.FILE => "File",
                        HpkgFileType.DIRECTORY => "Directory",
                        HpkgFileType.SYMLINK => "Symlink",
                        _ => "????"
                    };
                }));
            _treeView_PackageContents.AppendColumn(
                CreateTreeViewTextColumn("Date Modified", 3,
                (column, cell, model, iter) =>
                {
                    var entry = (HpkgDirectoryEntry)model.GetValue(iter, 0);
                    (cell as CellRendererText).Text = entry.FileModifiedTime?.ToString();
                }));
            _treeView_PackageContents.AppendColumn(
                CreateTreeViewTextColumn("Date Created", 4,
                (column, cell, model, iter) =>
                {
                    var entry = (HpkgDirectoryEntry)model.GetValue(iter, 0);
                    (cell as CellRendererText).Text = entry.FileCreationTime?.ToString();
                }));
            _treeView_PackageContents.AppendColumn(
                CreateTreeViewTextColumn("Date Accessed", 5,
                (column, cell, model, iter) =>
                {
                    var entry = (HpkgDirectoryEntry)model.GetValue(iter, 0);
                    (cell as CellRendererText).Text = entry.FileAccessTime?.ToString();
                }));

            _treeView_PackageContents.Selection.Changed += TreeView_PackageContents_SelectionChanged;

            _treeView_PackageContents.Model = _treeStore_Files;
        }

        private void Button_MoreProperties_Clicked(object sender, EventArgs e)
        {
            if (_currentPkg == null)
            {
                return;
            }

            new MoreInfoWindow(_currentPkg).Show();
        }

        private void ImageMenuItem_SaveAs_Clicked(object sender, EventArgs e)
        {
            if (_currentPkg == null)
            {
                return;
            }

            using var dialog = new FileChooserDialog("Open a HPKG file", this, FileChooserAction.Save,
                "Cancel", ResponseType.Cancel,
                "Save", ResponseType.Accept,
                null);
            if (dialog.Run() == (int)ResponseType.Accept)
            {
                var hpkgFile = dialog.Filename;
                try
                {
                    HpkgWriter.Write(_currentPkg, hpkgFile);
                    using var messageDialog = new MessageDialog(this, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok, "File successfully saved.");
                    messageDialog.Run();
                }
                catch (Exception ex)
                {
                    using var messageDialog = new MessageDialog(this, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, $"Failed to save file: {ex}");
                    messageDialog.Run();
                }
            }
        }

        private void Button_FilePreview_Clicked(object sender, EventArgs e)
        {
            _treeStore_Files.GetIter(out var iter,
                _treeView_PackageContents.Selection.GetSelectedRows()[0]);

            var entry = _treeStore_Files.GetValue(iter, 0) as HpkgDirectoryEntry;

            var contents = Encoding.Default.GetString(entry?.Data?.Read() ?? Array.Empty<byte>());

            using var dialog = new FilePreviewDialog(contents)
            {
                Title = $"{Title} Preview: {entry.Name}"
            };
            dialog.Run();
            dialog.Destroy();
        }

        private void TreeView_PackageContents_SelectionChanged(object sender, EventArgs e)
        {
            _button_FilePreview.Sensitive =
                _treeView_PackageContents.Selection.CountSelectedRows() != 0 &&
                _treeStore_Files.GetIter(out var iter, _treeView_PackageContents.Selection.GetSelectedRows()[0]) &&
                (_treeStore_Files.GetValue(iter, 0) as HpkgDirectoryEntry)?.FileType == HpkgFileType.FILE;

            // You can still export directories and symlinks.
            _button_FileExport.Sensitive = _treeView_PackageContents.Selection.CountSelectedRows() != 0;
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
                var pkg = new BetterPkg(hpkgFile);
                var success = true;

                try
                {
                    _entry_Name.Text = pkg.Name;
                    _entry_Arch.Text = pkg.Architecture.ToString();
                    _entry_Version.Text = pkg.BetterVersion.ToString();
                    _entry_Vendor.Text = pkg.Vendor;
                    _entry_Packager.Text = pkg.Packager;
                    _entry_BasePackage.Text = string.IsNullOrEmpty(pkg.BasePackage) ? "<none>" : pkg.BasePackage;
                    _entry_Flags.Text = pkg.Flags.ToString();
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

                    _comboBox_HomePageUrls.RemoveAll();
                    foreach (var url in pkg.HomePageUrls)
                    {
                        _comboBox_HomePageUrls.AppendText(url.ToString());
                    }
                    _comboBox_HomePageUrls.Active = 0;

                    _comboBox_SourceUrls.RemoveAll();
                    foreach (var url in pkg.SourceUrls)
                    {
                        _comboBox_SourceUrls.AppendText(url.ToString());
                    }
                    _comboBox_SourceUrls.Active = 0;

                    _treeStore_Files.Clear();
                    ReadHpkgFiles(pkg);
                }
                catch (Exception)
                {
                    success = false;
                }

                if (success)
                {
                    _currentPkg?.Dispose();
                    _currentPkg = pkg;
                }
            }
            dialog.Destroy();
        }

        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            _currentPkg?.Dispose();
            Application.Quit();
        }

        private void ReadHpkgFiles(BetterPkg pkg)
        {
            void IterateDirectory(TreeIter iter, List<HpkgDirectoryEntry> entries)
            {
                foreach (var entry in entries)
                {
                    var childIter = _treeStore_Files.AppendValues(iter, entry);
                    if (entry.FileType == HpkgFileType.DIRECTORY)
                    {
                        IterateDirectory(childIter, entry.Children);
                    }
                }
            }

            foreach (var entry in pkg.DirectoryEntries)
            {
                var childIter = _treeStore_Files.AppendValues(entry);
                if (entry.FileType == HpkgFileType.DIRECTORY)
                {
                    IterateDirectory(childIter, entry.Children);
                }
            }
        }
    }
}
