using Gtk;

namespace HpkgReader.Gtk
{
    internal static class LinkButtonHelpers
    {
        public static void EnsureLeftAlignment(this LinkButton button)
        {
            if (button.Child is Label label)
            {
                label.Xalign = 0.0f;
            }
        }
    }
}
