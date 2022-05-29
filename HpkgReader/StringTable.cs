/*
 * Copyright 2018, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

namespace HpkgReader
{
    /// <summary>
    /// The attribute-reading elements of the system need to be able to access a string table. This is interface of
    /// an object which is able to provide those strings.
    /// </summary>
    public abstract class StringTable
    {
        /// <summary>
        /// Given the index supplied, this method should return the corresponding string. It will throw an instance 
        /// of <see cref="HpkException"/> if there is any problems associated with achieving this.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public abstract string GetString(int index);
    }
}