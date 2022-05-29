using System;
using System.Collections.Generic;
using System.Text;

namespace HpkgReader.Compat
{
    internal static class OptionalCompat
    {
        public static T Map<S, T>(this S obj, Func<S, T> func)
            where S: class
            where T: class
        {
            return (obj == null) ? null : func(obj);
        }

        public static bool IsPresent<T>(this T obj)
            where T: class
        {
            return obj != null;
        }

        public static T Get<T>(this T obj)
        {
            return obj;
        }
    }
}
