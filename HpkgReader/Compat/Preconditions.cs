using System;

namespace HpkgReader.Compat
{
    internal class Preconditions
    {
        public static void CheckArgument(bool expression)
        {
            if (!expression)
            {
                throw new ArgumentException();
            }
        }

        public static void CheckArgument(bool expression, object errorMessage)
        {
            if (!expression)
            {
                throw new ArgumentException(errorMessage?.ToString());
            }
        }

        public static void CheckState(bool expression)
        {
            if (!expression)
            {
                throw new InvalidOperationException();
            }
        }

        public static void CheckState(bool expression, object errorMessage)
        {
            if (!expression)
            {
                throw new InvalidOperationException(errorMessage.ToString());
            }
        }

        public static T CheckNotNull<T>(T reference)
            where T : class
        {
            if (reference is null)
            {
                throw new NullReferenceException();
            }

            return reference;
        }
    }
}
