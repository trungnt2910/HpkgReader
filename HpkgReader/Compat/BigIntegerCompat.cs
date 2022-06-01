using System;
using System.Numerics;

namespace HpkgReader.Compat
{
    /// <summary>
    /// Compatibility functions for Java's BigInteger
    /// </summary>
    internal static class BigIntegerCompat
    {
        /// <summary>
        /// <para>Translates a byte array containing the two's-complement binary representation of a BigInteger into a BigInteger.</para>
        /// <para>The input array is assumed to be in big-endian byte-order: the most significant byte is in the zeroth element.</para>
        /// </summary>
        /// <param name="val">big-endian two's-complement binary representation of BigInteger.</param>
        /// <returns>A .NET <see cref="BigInteger"/> value</returns>
        /// <remarks>
        /// This method is needed because Java's <c>BigInteger</c> takes arrays in big-endian byte-order,
        /// while .NET accepts little-endian byte order.
        /// </remarks>
        public static BigInteger Construct(byte[] val)
        {
            var copy = new byte[val.Length];
            Array.Copy(val, copy, val.Length);
            Array.Reverse(copy);
            return new BigInteger(copy);
        }

        /// <summary>
        /// <para>Translates the sign-magnitude representation of a BigInteger into a BigInteger.</para>
        /// <para>The sign is represented as an integer signum value: -1 for negative, 0 for zero, or 1 for positive.</para>
        /// <para>The magnitude is a byte array in big-endian byte-order: the most significant byte is in the zeroth element.</para>
        /// <para>A zero-length magnitude array is permissible, and will result in a BigInteger value of 0, whether signum is -1, 0 or 1.</para>
        /// </summary>
        /// <param name="signum">signum of the number (-1 for negative, 0 for zero, 1 for positive).</param>
        /// <param name="magnitude">big-endian binary representation of the magnitude of the number.</param>
        /// <returns>A .NET <see cref="BigInteger"/> value</returns>
        /// <exception cref="InvalidOperationException">signum is not one of the three legal values (-1, 0, and 1), or signum is 0 and magnitude contains one or more non-zero bytes.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="signum"/> is null</exception>
        public static BigInteger Construct(int signum, byte[] magnitude)
        {
            if (signum < -1 || signum > 1)
            {
                throw new InvalidOperationException($"{nameof(signum)} is not one of three legal values (-1, 0, and 1)");
            }

            if (magnitude == null)
            {
                throw new ArgumentNullException($"{nameof(magnitude)} is null");
            }

            if (magnitude.Length == 0)
            {
                return BigInteger.Zero;
            }

            var copy = new byte[magnitude.Length + 1];
            Array.Copy(magnitude, 0, copy, 1, magnitude.Length);
            Array.Reverse(copy);

            var result = new BigInteger(copy);
            if (!result.IsZero && signum == 0)
            {
                throw new InvalidOperationException($"{nameof(signum)} is 0 and magnitude contains one or more non-zero bytes");
            }

            return signum * result;
        }

        /// <summary>
        /// Translates the decimal String representation of a BigInteger into a BigInteger. 
        /// The String representation consists of an optional minus sign followed by a sequence of one or more decimal digits. 
        /// The character-to-digit mapping is provided by Character.digit. 
        /// The String may not contain any extraneous characters (whitespace, for example).
        /// </summary>
        /// <param name="val">decimal String representation of BigInteger.</param>
        /// <returns></returns>
        public static BigInteger Construct(string val)
        {
            return BigInteger.Parse(val);
        }

        /// <summary>
        /// Converts this BigInteger to an int. This conversion is analogous to a narrowing primitive conversion from long to int as defined in section 5.1.3 of The Java™ Language Specification:
        /// if this BigInteger is too big to fit in an int, only the low-order 32 bits are returned. Note that this conversion can lose information about the overall magnitude of the BigInteger
        /// value as well as return a result with the opposite sign.
        /// </summary>
        /// <returns>this <see cref="BigInteger"/> converted to an <see cref="int"/>.</returns>
        public static int IntValue(this BigInteger number)
        {
            if (number > int.MaxValue || number < int.MinValue)
            {
                var bytes = number.ToByteArray();
                return BitConverter.ToInt32(bytes, 0);
            }
            return (int)number;
        }

        /// <summary>
        /// Converts this BigInteger to a long. This conversion is analogous to a narrowing primitive conversion from long to int as defined in section 5.1.3 of The Java™ Language Specification:
        /// if this BigInteger is too big to fit in a long, only the low-order 64 bits are returned. Note that this conversion can lose information about the overall magnitude of the BigInteger
        /// value as well as return a result with the opposite sign.
        /// </summary>
        /// <returns>this <see cref="BigInteger"/> converted to a <see cref="long"/>.</returns>
        public static long LongValue(this BigInteger number)
        {
            if (number > long.MaxValue || number < long.MinValue)
            {
                var bytes = number.ToByteArray();
                return BitConverter.ToInt64(bytes, 0);
            }
            return (long)number;
        }

        /// <summary>
        /// Returns a BigInteger whose value is (this / val).
        /// </summary>
        /// <param name="val">value by which this BigInteger is to be divided.</param>
        /// <returns>this / <paramref name="val"/></returns>
        public static BigInteger Divide(this BigInteger number, BigInteger val)
        {
            return number / val;
        }

        /// <summary>
        /// Returns a BigInteger whose value is (this * val).
        /// </summary>
        /// <param name="val">value to be multiplied by this BigInteger.</param>
        /// <returns>this * <paramref name="val"/></returns>
        public static BigInteger Multiply(this BigInteger number, BigInteger val)
        {
            return number * val;
        }
    }
}
