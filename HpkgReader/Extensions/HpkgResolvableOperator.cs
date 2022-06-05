namespace HpkgReader.Extensions
{
    /// <summary>
    /// Comparison operator for versions.
    /// See Haiku's header: os/package/PackageResolvableOperator.h
    /// </summary>
    public enum HpkgResolvableOperator: uint
    {
        /// <summary>
        /// less than the specified version
        /// </summary>
        Less = 0,
        /// <summary>
        /// less than or equal to the specified version
        /// </summary>
        LessEqual = 1,
        /// <summary>
        /// equal to the specified version
        /// </summary>
        Equal = 2,
        /// <summary>
        /// not equal to the specified version
        /// </summary>
        NotEqual = 3,
        /// <summary>
        /// greater than or equal to the specified version
        /// </summary>
        GreaterEqual = 4,
        /// <summary>
        /// greater than the specified version
        /// </summary>
        Greater = 5
    }
}
