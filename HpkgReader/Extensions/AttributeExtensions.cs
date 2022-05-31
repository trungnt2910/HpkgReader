using HpkgReader.Model;

namespace HpkgReader.Extensions
{
    public static class AttributeExtensions
    {
        public static T GetValue<T>(this Attribute attr, AttributeContext context)
        {
            return (T)attr.GetValue(context);
        }
    }
}
