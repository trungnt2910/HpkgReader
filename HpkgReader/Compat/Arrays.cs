namespace HpkgReader.Compat
{
    internal class Arrays
    {
        public static int HashCode(byte[] array)
        {
            if (array == null)
            {
                return 0;
            }
            int hashCode = 1;
            foreach (byte element in array)
            {
                // the hash code value for byte value is its integer value
                hashCode = 31 * hashCode + element;
            }
            return hashCode;
        }
    }
}
