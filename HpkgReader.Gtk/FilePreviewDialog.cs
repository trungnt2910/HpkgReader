using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace HpkgReader.Gtk
{
    class FilePreviewDialog : Dialog
    {
        [UI("TextView_FileContents")] private TextView _textView_FileContents = null;

        public FilePreviewDialog(string fileContents) : this(new Builder("FilePreviewDialog.glade"), fileContents) { }

        private FilePreviewDialog(Builder builder, string fileContents) : base(builder.GetRawOwnedObject("FilePreviewDialog"))
        {
            builder.Autoconnect(this);
            DefaultResponse = ResponseType.Cancel;
            Modal = true;

            _textView_FileContents.Buffer.Text = fileContents;

            Response += Dialog_Response;
        }

        public string FileContents
        {
            get => _textView_FileContents?.Buffer.Text;
            set
            {
                _textView_FileContents.Buffer.Text = value;
            }
        }

        private void Dialog_Response(object o, ResponseArgs args)
        {
            Hide();
        }
    }
}
