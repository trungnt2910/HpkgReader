using System.Collections.Generic;

namespace HpkgReader.Compat
{
    internal static class ListCompat
    {
        public static bool IsEmpty<T>(this IList<T> list)
        {
            return list.Count == 0;
        }

        public static int Size<T>(this IList<T> list)
        {
            return list.Count;
        }

        public static List<T> SubList<T>(this List<T> list, int fromIndex, int toIndex)
        {
            return list.GetRange(fromIndex, toIndex - fromIndex);
        }
    }
}
